using System.Net.Http;
using TemperatureSensorAndroid.Models;
using TemperatureSensorAndroid.Services.Abstract;
using TemperatureSensorAndroid.Services.Concrete;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TemperatureSensorAndroid.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IRequestSenderService _requestSenderService;
        private readonly AppConfiguration _appConfiguration;

        public SettingsViewModel()
        {
            Title = "Настройки";

            _appConfiguration = (AppConfiguration)Application.Current.Resources["AppConfiguration"];

            var httpClient = new HttpClient();
            _requestSenderService = new RequestSenderService(httpClient);
            _requestSenderService.SetAuthorizationHeader(_appConfiguration.Login, _appConfiguration.Password);

            var settings = _requestSenderService.GetJson<Settings>($"{_appConfiguration.BaseUrlApi}{_appConfiguration.Settings}");

            DelayMinutes = settings.DelayMinutes;
            SendSmsMaxDay = settings.SendSmsMaxDay;
            TemperatureMin = settings.TemperatureMin;
            TemperatureMax = settings.TemperatureMax;
            UrlChart = settings.UrlChart;
            UrlTable = settings.UrlTable;

            SaveCommand = new Command(OnSave, ValidateSave);
        }

        private bool ValidateSave()
        {
            return true;
        }

        string urlChart = string.Empty;
        public string UrlChart
        {
            get { return urlChart; }
            set { SetProperty(ref urlChart, value); }
        }

        string urlTable = string.Empty;
        public string UrlTable
        {
            get { return urlTable; }
            set { SetProperty(ref urlTable, value); }
        }

        int delayMinutes = 5;
        public int DelayMinutes
        {
            get { return delayMinutes; }
            set { SetProperty(ref delayMinutes, value); }
        }

        int sendSmsMaxDay = 5;
        public int SendSmsMaxDay
        {
            get { return sendSmsMaxDay; }
            set { SetProperty(ref sendSmsMaxDay, value); }
        }

        int temperatureMin = 10;
        public int TemperatureMin
        {
            get { return temperatureMin; }
            set { SetProperty(ref temperatureMin, value); }
        }

        int temperatureMax = 40;
        public int TemperatureMax
        {
            get { return temperatureMax; }
            set { SetProperty(ref temperatureMax, value); }
        }

        public Command SaveCommand { get; }

        private void OnSave()
        {
            var settings = new Settings
            {
                DelayMinutes = delayMinutes,
                SendSmsMaxDay = sendSmsMaxDay,
                TemperatureMin = temperatureMin,
                TemperatureMax = temperatureMax,
                UrlChart = urlChart,
                UrlTable = urlTable
            };

            Preferences.Set("url_chart", urlChart);
            Preferences.Set("url_temperature_table", urlTable);

            _requestSenderService.PutJson($"{_appConfiguration.BaseUrlApi}{_appConfiguration.Settings}", settings);
        }
    }
}
