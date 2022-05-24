// Please select the corresponding model
// Select your modem:
#define TINY_GSM_MODEM_SIM800

#define SIM800L_IP5306_VERSION_20190610
// #define SIM800L_AXP192_VERSION_20200327
// #define SIM800C_AXP192_VERSION_20200609
// #define SIM800L_IP5306_VERSION_20200811

#include <Arduino.h>
#include "utilities.h"
#include <iarduino_AM2320.h>
#include <SoftwareSerial.h>
#include <ArduinoJson.h>

// Enable library for reading and writing from/to flash memory
#include <EEPROM.h>

// Set serial for debug console (to the Serial Monitor, default speed 115200)
#define SerialMon Serial
// Set serial for AT commands (to the module)
#define SerialAT  Serial1

// See all AT commands, if wanted
#define DUMP_AT_COMMANDS

// Define the serial console for debug prints, if needed
#define TINY_GSM_DEBUG SerialMon
// #define LOGGING  // <- Logging is for the HTTP library

// Range to attempt to autobaud
// NOTE:  DO NOT AUTOBAUD in production code.  Once you've established
// communication, set a fixed baud rate using modem.setBaud(#).
#define AUTOBAUD_MIN 9600
#define AUTOBAUD_MAX 115200

// Add a reception delay, if needed.
// This may be needed for a fast processor at a slow baud rate.
// #define TINY_GSM_YIELD() { delay(2); }

// Define how you're planning to connect to the internet.
// This is only needed for this example, not in other code.
#define TINY_GSM_USE_GPRS true
#define TINY_GSM_USE_WIFI false

// Set GSM PIN, if any
#define SIM_PIN ""

// Define the number of bytes you want to access
#define EEPROM_SIZE 3

// Flag to force SSL client authentication, if needed
// #define TINY_GSM_SSL_CLIENT_AUTHENTICATION

#define APN_NAME ""
#define APN_USER ""
#define APN_PSWD ""

// Your GPRS credentials, if any
const char apn[]      = "YourAPN";
const char gprsUser[] = "";
const char gprsPass[] = "";

// Authorization basic
#define AUTHORIZATION_VALUE "Basic YWRtaW46YWRtaW4="

// Web server
#define WEB_SERVER "jeanrasin-001-site1.itempurl.com"

// API add temperature
#define WEB_SERVER_URL_POST_TEMPERATURE "/api/Temperature"

// API GET settings
#define WEB_SERVER_URL_GET_SETTINGS "/api/Settings"

// Web server port
#define WEB_SERVER_PORT 80

// Replace with the number of the controlling phone
#define PHONE_NUMBER "+79197181453"

#include <TinyGsmClient.h>
#include <ArduinoHttpClient.h>

// Just in case someone defined the wrong thing..
#if TINY_GSM_USE_GPRS && not defined TINY_GSM_MODEM_HAS_GPRS
#undef TINY_GSM_USE_GPRS
#undef TINY_GSM_USE_WIFI
#define TINY_GSM_USE_GPRS false
#define TINY_GSM_USE_WIFI true
#endif
#if TINY_GSM_USE_WIFI && not defined TINY_GSM_MODEM_HAS_WIFI
#undef TINY_GSM_USE_GPRS
#undef TINY_GSM_USE_WIFI
#define TINY_GSM_USE_GPRS true
#define TINY_GSM_USE_WIFI false
#endif

// Delay minutes
int delayMinutes = 5;

// Max SMS per day
int sendSmsMaxDay = 2;

// Minimum temperature for warning
int temperatureMin = 10;

// Maximum temperature for warning
int temperatureMax = 40;

// Your WiFi connection credentials, if applicable
const char wifiSSID[] = "YourSSID";
const char wifiPass[] = "YourWiFiPass";

struct Date {
  byte year, month, day, hour;
};

#ifdef DUMP_AT_COMMANDS
#include <StreamDebugger.h>
StreamDebugger debugger(SerialAT, SerialMon);
TinyGsm modem(debugger);
#else
TinyGsm modem(SerialAT);
#endif

TinyGsmClient clientGSM(modem);

HttpClient* http;

// Initializing the sensor
iarduino_AM2320 sensor;

void SetupModem();
void InitGSM_SIM800();
String SensorRead();
void RestartGSMModem();
void SendSms(String smsMessage, int day, int hour, int count);
void SendSmsVerification(String smsMessage);
void SetAutoTimeSync();
String GetDateTime();
Date GetDate();
String GSMSignalLevel(int level);
String GSMRegistrationStatus(RegStatus state);
String SIMStatus(SimStatus state);
String SwapLocation(String location);
String SendDataPOST(String server, String url, int port, String authorizationValue, String postData);
String SendDataGET(String server, String url, int port, String authorizationValue);
void SendTemperature(String server, String url, int port, float temperature, float humidity);
void SetSettings(String json);
void GetSettings(String server, String url, int port, String authorizationValue);

void setup() {

  // Sensor begin
  sensor.begin();

  InitGSM_SIM800();

  // Initialize EEPROM with predefined size
  EEPROM.begin(EEPROM_SIZE);

  // Synchronizing time
  SetAutoTimeSync();

  delay(3000);
 
#if TINY_GSM_USE_WIFI
  // Wifi connection parameters must be set before waiting for the network
  Serial.print(F("Setting SSID/password..."));
  if (!modem.networkConnect(wifiSSID, wifiPass)) {
    Serial.println(" fail");
    delay(10000);
    return;
  }
  Serial.println(" success");
#endif

#if TINY_GSM_USE_GPRS && defined TINY_GSM_MODEM_XBEE
  // The XBee must run the gprsConnect function BEFORE waiting for network!
  modem.gprsConnect(apn, gprsUser, gprsPass);
#endif

  Serial.print("Waiting for network...");
  if (!modem.waitForNetwork()) {
    Serial.println(" fail");
    delay(10000);
    return;
  }
  Serial.println(" success");

  if (modem.isNetworkConnected()) {
    Serial.println("Network connected");
  }

#if TINY_GSM_USE_GPRS
  // GPRS connection parameters are usually set after network registration
  Serial.print(F("Connecting to "));
  Serial.print(apn);
  if (!modem.gprsConnect(apn, gprsUser, gprsPass)) {
    Serial.println(" fail");
    delay(10000);
    return;
  }
  Serial.println(" success");

  if (modem.isGprsConnected()) {
    Serial.println("GPRS connected");
  }
#endif

  GetSettings(WEB_SERVER, WEB_SERVER_URL_GET_SETTINGS, WEB_SERVER_PORT, AUTHORIZATION_VALUE);

  // Clear flash memory
  // EEPROM.write(0, 0);
  // EEPROM.write(1, -1);
  // EEPROM.write(2, 0);
  // EEPROM.commit();

  delay(3000);

  Serial.println("------------------");
}

void loop() {
 
  String message = SensorRead();
 
  if (message != "") {
    Serial.println(message);
    SendSmsVerification(message);
  }

  if (sensor.tem <= temperatureMin) {
    String smsMessage = "Warning. Temperature below " + String(temperatureMin) + " degrees.";
    SendSmsVerification(smsMessage);
  }

  if (sensor.tem > temperatureMax) {
    String smsMessage = "Warning. Temperature abow " + String(temperatureMax) + " degrees.";
    SendSmsVerification(smsMessage);
  }

  SendTemperature(WEB_SERVER, WEB_SERVER_URL_POST_TEMPERATURE, WEB_SERVER_PORT, sensor.tem, sensor.hum);

  // Delay in milliseconds
  delay(delayMinutes * 60 * 1000);
}


void SetupModem() {
#ifdef MODEM_RST
  // Keep reset high
  pinMode(MODEM_RST, OUTPUT);
  digitalWrite(MODEM_RST, HIGH);
#endif

  pinMode(MODEM_PWRKEY, OUTPUT);
  pinMode(MODEM_POWER_ON, OUTPUT);

  // Turn on the Modem power first
  digitalWrite(MODEM_POWER_ON, HIGH);

  // Pull down PWRKEY for more than 1 second according to manual requirements
  digitalWrite(MODEM_PWRKEY, HIGH);
  delay(100);
  digitalWrite(MODEM_PWRKEY, LOW);
  delay(1000);
  digitalWrite(MODEM_PWRKEY, HIGH);

  // Initialize the indicator as an output
  pinMode(LED_GPIO, OUTPUT);
  digitalWrite(LED_GPIO, LED_OFF);
}

void InitGSM_SIM800() {
  // Set console baud rate
  Serial.begin(AUTOBAUD_MAX);

  delay(10);

  Serial.println("Initialize GSM modem...");

  // Set GSM module baud rate
  // Set GSM module baud rate and UART pins
  SerialAT.begin(AUTOBAUD_MAX, SERIAL_8N1, MODEM_RX, MODEM_TX);
  Serial.println("Serial GSM Txd is on GPIO" + String(MODEM_TX));
  Serial.println("Serial GSM Rxd is on GPIO" + String(MODEM_RX));

  SetupModem();
  delay(6000);

  String info = modem.getModemInfo();

  Serial.println(info);

  if (!modem.restart())
  {
    RestartGSMModem();
  }
  else
  {
    Serial.println("Modem restart OK");
  }

  if (modem.getSimStatus() != SIM_READY)
  {
    Serial.println("Check PIN code for the SIM. SIM status: " + SimStatus(modem.getSimStatus()));
    if (SIM_PIN != "")
    {
      Serial.println("Try to unlock SIM PIN.");
      modem.simUnlock(SIM_PIN);
      delay(3000);
      if (modem.getSimStatus() != 3)
      {
        RestartGSMModem();
      }
    }
  }

  if (!modem.waitForNetwork())
  {
    Serial.println("Failed to connect to network");
    RestartGSMModem();
  }
  else
  {
    RegStatus registration = modem.getRegistrationStatus();
    Serial.println("Registration: [" + GSMRegistrationStatus(registration) + "]");
    Serial.println("Modem network OK");
  }

  bool isConnect = modem.gprsConnect(APN_NAME, APN_USER, APN_PSWD);
  //delay(10000);
  Serial.println(isConnect ? "GPRS Connect OK" : "GPRS Connection failed");

  bool stateGPRS = modem.isGprsConnected();
  if (!stateGPRS)
  {
    RestartGSMModem();
  }

  String state = stateGPRS ? "connected" : "not connected";
  Serial.println("GPRS status: " + state);
  Serial.println("CCID: " + modem.getSimCCID());
  Serial.println("IMEI: " + modem.getIMEI());
  Serial.println("Operator: " + modem.getOperator());

  IPAddress local = modem.localIP();
  Serial.println("Local IP: " + local.toString());

  int csq = modem.getSignalQuality();
  if (csq == 0)
  {
    Serial.println("Signal quality is 0. Restart modem.");
    RestartGSMModem();
  }
  Serial.println("Signal quality: " + GSMSignalLevel(csq) + " [" + String(csq) + "]");

  int battLevel = modem.getBattPercent();
  Serial.println("Battery level: " + String(battLevel) + "%");

  float battVoltage = modem.getBattVoltage() / 1000.0F;
  Serial.println("Battery voltage: " + String(battVoltage));

  String gsmLoc = modem.getGsmLocation();
  Serial.println("GSM location: " + gsmLoc);
  Serial.println("GSM location: " + SwapLocation(gsmLoc));
}

void RestartGSMModem() {
  Serial.println("Restarting GSM...");
  if (!modem.restart())
  {
    Serial.println("\tFailed. :-(\r\n");
    //ESP.restart();
  }
  InitGSM_SIM800();
}

void SendSms(String smsMessage, int day, int hour, int count) {

  // To send an SMS, call modem.sendSMS(SMS_TARGET, smsMessage)
  if (modem.sendSMS(PHONE_NUMBER, smsMessage)) {
    EEPROM.write(0, day);
    EEPROM.write(1, hour);
    EEPROM.write(2, count);
    EEPROM.commit();

    Serial.println(smsMessage);
  }
  else {
    Serial.println("SMS failed to send");
  }
}

void SendSmsVerification(String smsMessage) {
  byte day = EEPROM.read(0);
  byte hour = EEPROM.read(1);
  byte count = EEPROM.read(2);

  Serial.println("EEPROM. Day: " + String(day) + ", hour: " + String(hour) + ", count: " + String(count));

  Date date = GetDate();

  if (date.day == day && hour != date.hour && count < sendSmsMaxDay) {
    count += 1;
    SendSms(smsMessage, date.day, date.hour, count);
  } else if (date.day != day) {
    EEPROM.write(0, date.day);
    EEPROM.write(1, -1);
    EEPROM.write(2, 0);
    EEPROM.commit();
  }

}

void SetAutoTimeSync() {
  Serial.println("Enable GSM RTC network time sync.");
  modem.sendAT(GF("+CLTS=1"));
  if (modem.waitResponse(10000L) != 1) {
    Serial.println("AT command \"AT+CLTS=1\" executing fault.");;
  }

  modem.sendAT(GF("&W"));
  modem.waitResponse();

  modem.sendAT(GF("+CFUN=0"));
  if (modem.waitResponse(10000L) != 1) {
    Serial.println("AT command \"AT+CFUN=0\" executing fault.");;
  }

  modem.sendAT(GF("+CFUN=1"));
  if (modem.waitResponse(10000L, GF(GSM_NL "DST:")) != 1) {
    Serial.println("AT command \"AT+CFUN=1\" executing fault.");
  }

  Serial.println("DateTime: " + GetDateTime());
}

String GetDateTime() {
  modem.sendAT(GF("+CCLK?"));
  if (modem.waitResponse(10000L, GF(GSM_NL "+CCLK:"))) {
    String body = modem.stream.readStringUntil('\n');
    return body;
  }
  else {
    Serial.println("AT command \"+CCLK?\" executing fault.");
  }

  return "";
}

String SensorRead() {
  String message = "";
  switch (sensor.read()) {
    case AM2320_OK: // We read the sensor readings.
      Serial.println((String) "Sensor AM2320:  T=" + sensor.tem + "*C, PH=" + sensor.hum + "%");
      break;
    case AM2320_ERROR_LEN:  // The amount of transmitted data exceeds the I2C buffer.
      message = "Sending is not possible.";
      break;
    case AM2320_ERROR_ADDR: // NACK received while transmitting sensor address.
      message = "No sensor.";
      break;
    case AM2320_ERROR_DATA: // Received NACK while transmitting data to the sensor.
      message = "Sending is not possible.";
      break;
    case AM2320_ERROR_SEND: // Data transfer error.
      message = "Sending is not possible.";
      break;
    case AM2320_ERROR_READ: // Empty sensor response received.
      message = "No response from sensor.";
      break;
    case AM2320_ERROR_ANS:  // Sensor response does not match request.
      message = "Answer is incorrect.";
      break;
    case AM2320_ERROR_LINE: // Interference in the communication line (CRC does not match).
      message = "CRC does not match.";
      break;
  }

  return message;
}

Date GetDate() {
  // Return "22/04/26,10:59:21+20"
  String dateStr = GetDateTime();

  Date date;

  date.year = (dateStr.substring(2, 4)).toInt();
  date.month = (dateStr.substring(5, 7)).toInt();
  date.day = (dateStr.substring(8, 10)).toInt();
  date.hour = (dateStr.substring(11, 13)).toInt();

  Serial.println("DateTime: " + dateStr);
  Serial.println("Date. Year:" + String(date.year) + ", month:" + String(date.month) + ", day:" + String(date.day) + ", hour:" + String(date.hour));

  return date;
}

String GSMSignalLevel(int level) {
  switch (level) {
    case 0:
      return "-115 dBm or less";
    case 1:
      return "-111 dBm";
    case 31:
      return "-52 dBm or greater";
    case 99:
      return "not known or not detectable";
    default:
      if (level > 1 && level < 31)
        return "-110... -54 dBm";
  }
  return "Unknown";
}

String GSMRegistrationStatus(RegStatus state) {
  switch (state) {
    case REG_UNREGISTERED:
      return "Not registered, MT is not currently searching a new operator to register to";
    case REG_SEARCHING:
      return "Not registered, but MT is currently searching a new operator to register to";
    case REG_DENIED:
      return "Registration denied";
    case REG_OK_HOME:
      return "Registered, home network";
    case REG_OK_ROAMING:
      return "Registered, roaming";
    case REG_UNKNOWN:
      return "Unknown";
  }
  return "Unknown";
}

String SIMStatus(SimStatus state) {
  switch (state) {
    case SIM_ERROR:
      return "SIM card ERROR";
    case SIM_READY:
      return "SIM card is READY";
    case SIM_LOCKED:
      return "SIM card is LOCKED. PIN/PUK needed.";
  }
  return "Unknown STATUS";
}

String SwapLocation(String location) {
  int i = location.indexOf(',');
  int j = location.indexOf(',', i + 1);
  String longitude = location.substring(i + 1, j);
  i = location.indexOf(',', j + 1);
  String latitude = location.substring(j + 1, i);
  return latitude + "," + longitude;
}

String SendDataPOST(String server, String url, int port, String authorizationValue, String postData) {
  bool stateGPRS = modem.isGprsConnected();
  String res = "";
  if (stateGPRS) {
    http = new HttpClient(clientGSM, server, port);
    Serial.println("Send POST request...");
    http->beginRequest();
    http->post(url);
    http->sendHeader("Content-Type", "application/x-www-form-urlencoded");
    http->sendHeader("Content-Length", postData.length());
    http->sendHeader("Authorization", authorizationValue);
    http->beginBody();
    http->print(postData);
    http->endRequest();

    // read the status code and body of the response
    int statusCode = http->responseStatusCode();
    if (statusCode == 200) {
      String response = http->responseBody();
      Serial.println("Status code: " + String(statusCode));
      Serial.println("Response: " + response);
      res = response;
    }
    else {
      Serial.println("Error code: " + String(statusCode));
    }

    http->stop();
  }

  return res;
}

String SendDataGET(String server, String url, int port, String authorizationValue) {
  Serial.println("+++00");
  bool stateGPRS = modem.isGprsConnected();
  String res = "";
  if (stateGPRS) {
    Serial.println("+++11");
    http = new HttpClient(clientGSM, server, port);
    Serial.println("Send GET request...");
    http->beginRequest();
    http->get(url);
    http->sendHeader("Authorization", authorizationValue);
    http->beginBody();
    http->endRequest();

    // read the status code and body of the response
    int statusCode = http->responseStatusCode();
    if (statusCode == 200) {
      String response = http->responseBody();
      Serial.println("Status code: " + String(statusCode));
      Serial.println("Response: " + response);
      res = response;
    }
    else {
      Serial.println("Error code: " + String(statusCode));
    }

    http->stop();
  }

  return res;
}

void SendTemperature(String server, String url, int port, float temperature, float humidity) {
  String postData = "temperature=" + String(temperature);
  postData += "&humidity=" + String(humidity);

  Serial.println("PostData: " + postData);

  String json = SendDataPOST(server, url, port, AUTHORIZATION_VALUE, postData);
  Serial.println("JSON: " + String(json));

  if (json == "") {
    String smsMessage = "Warning. Web server not responding.";
    SendSmsVerification(smsMessage);
  }
  else {
    SetSettings(json);
  }

}

void SetSettings(String json) {
  StaticJsonDocument<400> doc;
  deserializeJson(doc, json);

  delayMinutes = doc["delayMinutes"];
  sendSmsMaxDay = doc["sendSmsMaxDay"];
  temperatureMin = doc["temperatureMin"];
  temperatureMax = doc["temperatureMax"];

  Serial.println("delayMinutes: " + String(delayMinutes));
  Serial.println("sendSmsMaxDay: " + String(sendSmsMaxDay));
  Serial.println("temperatureMin: " + String(temperatureMin));
  Serial.println("temperatureMax: " + String(temperatureMax));
}

void GetSettings(String server, String url, int port, String authorizationValue) {
  String json = SendDataGET(server, url, port, authorizationValue);
  Serial.println("JSON: " + String(json));

  if (json != "") {
    SetSettings(json);
  }
}
