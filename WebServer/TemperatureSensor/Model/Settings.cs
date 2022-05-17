namespace TemperatureSensor.WebUI.Model
{
    public class Settings
    {
        public int DelayMinutes { get; set; }

        public int SendSmsMaxDay { get; set; }

        public int TemperatureMin { get; set; }

        public int TemperatureMax { get; set; }

        public string UrlChart { get; set; }

        public string UrlTable { get; set; }
    }
}