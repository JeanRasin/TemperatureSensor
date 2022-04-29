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
//#define AUTOBAUD_MIN 9600
#define AUTOBAUD_MAX 115200

// Add a reception delay, if needed.
// This may be needed for a fast processor at a slow baud rate.
// #define TINY_GSM_YIELD() { delay(2); }

// Define how you're planning to connect to the internet.
// This is only needed for this example, not in other code.
#define TINY_GSM_USE_GPRS true
#define TINY_GSM_USE_WIFI false

// Set GSM PIN, if any
#define GSM_PIN ""

// Define the number of bytes you want to access
#define EEPROM_SIZE 3

// Delay minutes
#define DELAY_MINUTES 1

// Max SMS per day
#define SEND_SMS_MAX_DAY 2

// Minimum temperature for warning
#define TEMPERATURE_MIN 10

// Maximum temperature for warning
#define TEMPERATURE_MAX 40

// Flag to force SSL client authentication, if needed
// #define TINY_GSM_SSL_CLIENT_AUTHENTICATION

// Your GPRS credentials, if any
const char apn[]      = "YourAPN";
const char gprsUser[] = "";
const char gprsPass[] = "";

// Your WiFi connection credentials, if applicable
const char wifiSSID[] = "YourSSID";
const char wifiPass[] = "YourWiFiPass";

// Authorization basic
const String authorizationValue = "Basic YWRtaW46YWRtaW4=";

// Web server url
const String webServerUrl = "http://jeanrasin-001-site1.itempurl.com/api/Temperature/temperature/{temperature}/humidity/{humidity}";

struct Date {
  byte year, month, day, hour;
};

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

#ifdef DUMP_AT_COMMANDS
#include <StreamDebugger.h>
StreamDebugger debugger(SerialAT, SerialMon);
TinyGsm modem(debugger);
#else
TinyGsm modem(SerialAT);
#endif

// Initializing the sensor
iarduino_AM2320 sensor;

// Replace with the number of the controlling phone
String myPhoneNum = "+79197181453";

void setupModem()
{
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

bool BearingSet();
bool HttpTemperature(float temperature, float humidity);
bool CloseServe();
void SendSms(String smsMessage, int day, int hour, int count);
void SendSmsVerification(String smsMessage);
void SetAutoTimeSync();
String GetDateTime();
Date GetDate();
void GetHttpResponse();

void setup() {

  // Sensor begin
  sensor.begin();

  // Set console baud rate
  Serial.begin(AUTOBAUD_MAX);

  delay(10);

  // Set GSM module baud rate
  // Set GSM module baud rate and UART pins
  SerialAT.begin(AUTOBAUD_MAX, SERIAL_8N1, MODEM_RX, MODEM_TX);

  setupModem();
  delay(6000);

  // Restart takes quite some time
  // To skip it, call init() instead of restart()
  Serial.println("Initializing modem...");
  modem.restart();
  // modem.init();

  String modemInfo = modem.getModemInfo();
  Serial.print("Modem Info: ");
  Serial.println(modemInfo);

#if TINY_GSM_USE_GPRS
  // Unlock your SIM card with a PIN if needed
  if (GSM_PIN && modem.getSimStatus() != 3) {
    modem.simUnlock(GSM_PIN);
  }
#endif

  // Initialize EEPROM with predefined size
  EEPROM.begin(EEPROM_SIZE);

  // Synchronizing time
  SetAutoTimeSync();

  // Clear flash memory
  // EEPROM.write(0, 0);
  // EEPROM.write(1, -1);
  // EEPROM.write(2, 0);
  // EEPROM.commit();
}

void loop() {
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

  switch (sensor.read()) {                                                      // We read the sensor readings.
    case AM2320_OK:         Serial.println((String) "Sensor AM2320:  T=" + sensor.tem + "*C, PH=" + sensor.hum + "%");  break;
    case AM2320_ERROR_LEN:  Serial.println("Sending is not possible.");     break;     // The amount of transmitted data exceeds the I2C buffer.
    case AM2320_ERROR_ADDR: Serial.println("No sensor.");           break;     // NACK received while transmitting sensor address.
    case AM2320_ERROR_DATA: Serial.println("Sending is not possible.");     break;     // Received NACK while transmitting data to the sensor.
    case AM2320_ERROR_SEND: Serial.println("Sending is not possible.");     break;     // Data transfer error.
    case AM2320_ERROR_READ: Serial.println("No response from sensor."); break;     // Empty sensor response received.
    case AM2320_ERROR_ANS:  Serial.println("Answer is incorrect.");     break;     // Sensor response does not match request.
    case AM2320_ERROR_LINE: Serial.println("CRC does not match.");       break;     // Interference in the communication line (CRC does not match).
  }

  Serial.print(F("Performing HTTP GET request... "));

  if (BearingSet() == false) Serial.println("Bearing set fall");

  if (HttpTemperature(sensor.tem, sensor.hum) == false) {
    Serial.println("Insert temperature fall");
  }

  CloseServe();

#if TINY_GSM_USE_WIFI
  modem.networkDisconnect();
  Serial.println(F("WiFi disconnected"));
#endif
#if TINY_GSM_USE_GPRS
  modem.gprsDisconnect();
  Serial.println(F("GPRS disconnected"));
#endif

  // Delay in milliseconds
  delay(DELAY_MINUTES * 60 * 1000);
}

//Bearing set
bool BearingSet() {

  // Configuring Bearer Scenarios
  modem.sendAT(GF("+HTTPTERM"));
  if (modem.waitResponse(10000L) != 1) {
    DBG(GF("+HTTPTERM"));
    //return false;
  }

  // Configuring Bearer Scenarios
  modem.sendAT(GF("+SAPBR=0,1"));
  if (modem.waitResponse(10000L) != 1) {
    DBG(GF("+SAPBR=0,1"));
    return false;
  }
  delay(1000);

  // Configuring Bearer Scenarios
  modem.sendAT(GF("+SAPBR=3,1,\"Contype\",\"GPRS\""));
  if (modem.waitResponse(10000L) != 1) {
    DBG(GF("+SAPBR=3,1,\"Contype\",\"GPRS\""));
    return false;
  }

  // Activate a GPRS context
  modem.sendAT(GF("+SAPBR=1,1"));
  if (modem.waitResponse(10000L) != 1) {
    DBG(GF("+SAPBR=1,1"));
    //return false;
  }

  // Query the GPRS context
  modem.sendAT(GF("+SAPBR=2,1"));
  if (modem.waitResponse(10000L) != 1) {
    DBG(GF("+SAPBR=2,1"));
    //return false;
  }

  delay(2000);
}

bool HttpTemperature(float temperature, float humidity) {

  // Initialize the HTTP service
  modem.sendAT(GF("+HTTPINIT"));
  if (modem.waitResponse(10000L) != 1) {
    DBG(GF("+HTTPINIT"));
    return false;
  }

  // Set HTTP session parameters
  modem.sendAT(GF("+HTTPPARA=\"CID\",1"));
  if (modem.waitResponse(10000L) != 1) {
    DBG(GF("+HTTPPARA=\"CID\",1"));
    return false;
  }

  // Set basic authorization
  modem.sendAT(GF("+HTTPPARA=\"USERDATA\",\"Authorization: " + authorizationValue + "\""));
  if (modem.waitResponse(10000L) != 1) {
    DBG(GF("+HTTPPARA=\"USERDATA\",\"Authorization: " + authorizationValue + "\""));
    return false;
  }

  String url = webServerUrl;

  url.replace("{temperature}", String(temperature, 1));
  url.replace("{humidity}", String(humidity, 1));

  // Set HTTP session parameters
  modem.sendAT(GF("+HTTPPARA=\"URL\",\"" + url + "\""));
  if (modem.waitResponse(10000L) != 1) {//, res
    DBG(GF("+HTTPPARA=\"URL\",\"" + url + "\""));
    return false;
  }

  // Post
  modem.sendAT(GF("+HTTPACTION=1"));
  if (modem.waitResponse(60000L) != 1) {
    DBG(GF("+HTTPACTION=1"));
    return false;
  }

  delay(1000);

  // Read data from the HTTP server
  modem.sendAT(GF("+HTTPREAD"));
  if (modem.waitResponse(60000L) != 1) {
    DBG(GF("+HTTPREAD"));
    return false;
  }
  delay(10000);

  GetHttpResponse();

  delay(1000);

  if (temperature <= TEMPERATURE_MIN) {
    String smsMessage = "Warning. Temperature below " + String(TEMPERATURE_MIN) + " degrees.";
    SendSmsVerification(smsMessage);
  }

  if (temperature > TEMPERATURE_MAX) {
    String smsMessage = "Warning. Temperature abow " + String(TEMPERATURE_MAX) + " degrees.";
    SendSmsVerification(smsMessage);
  }

}

bool CloseServe() {

  delay(5000);

  // Close http
  modem.sendAT(GF("+HTTPTERM"));
  if (modem.waitResponse(10000L) != 1) {
    DBG(GF("+HTTPTERM"));
    return false;
  }

  delay(5000);

  // Close GPRS
  modem.sendAT(GF("+SAPBR=0,1"));
  if (modem.waitResponse(10000L) != 1) {
    DBG(GF("+SAPBR=0,1"));
    return false;
  }
}

void SendSms(String smsMessage, int day, int hour, int count) {

  // To send an SMS, call modem.sendSMS(SMS_TARGET, smsMessage)
  if (modem.sendSMS(myPhoneNum, smsMessage)) {
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

  if (date.day == day && hour != date.hour && count < SEND_SMS_MAX_DAY) {
    count += 1;
    SendSms(smsMessage, date.day, date.hour, count);
  } else if (date.day != day) {
    EEPROM.write(0, date.day);
    EEPROM.write(1, -1);
    EEPROM.write(2, 0);
    EEPROM.commit();
  }

}

void SetAutoTimeSync()
{
  Serial.println("Enable GSM RTC network time sync.");
  modem.sendAT(GF("+CLTS=1"));
  if (modem.waitResponse(10000L) != 1)
  {
    Serial.println("AT command \"AT+CLTS=1\" executing fault.");;
  }

  modem.sendAT(GF("&W"));
  modem.waitResponse();

  modem.sendAT(GF("+CFUN=0"));
  if (modem.waitResponse(10000L) != 1)
  {
    Serial.println("AT command \"AT+CFUN=0\" executing fault.");;
  }

  modem.sendAT(GF("+CFUN=1"));
  if (modem.waitResponse(10000L, GF(GSM_NL "DST:")) != 1)
  {
    Serial.println("AT command \"AT+CFUN=1\" executing fault.");
  }

  Serial.println("DateTime: " + GetDateTime());
}

String GetDateTime()
{
  modem.sendAT(GF("+CCLK?"));
  if (modem.waitResponse(10000L, GF(GSM_NL "+CCLK:")))
  {
    String body = modem.stream.readStringUntil('\n');
    return body;
  }
  else
  {
    Serial.println("AT command \"+CCLK?\" executing fault.");
  }
  return "";
}

Date GetDate()
{
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

void GetHttpResponse()
{
  String httpResponse = "";

  if (SerialAT.available()) {
    httpResponse = SerialAT.readString();
  }

  // +HTTPACTION: 1,200,2
  String httpCode =  httpResponse.substring(17, 20);

  Serial.println("Http code: " + httpCode);

  if (httpCode != "200") {
    String smsMessage = "Warning. Web server not responding.";
    SendSmsVerification(smsMessage);
  }

}
