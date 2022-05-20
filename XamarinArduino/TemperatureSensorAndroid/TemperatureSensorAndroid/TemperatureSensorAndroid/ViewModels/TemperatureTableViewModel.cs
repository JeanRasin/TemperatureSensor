using Xamarin.Essentials;

namespace TemperatureSensorAndroid.ViewModels
{
    public class TemperatureTableViewModel : BaseViewModel
    {
        string urlPath = string.Empty;
        public string UrlPath
        {
            get { return urlPath; }
            set { SetProperty(ref urlPath, value); }
        }

        public TemperatureTableViewModel()
        {
            Title = "Таблица данных";
            OnAppearing();
        }

        public void OnAppearing()
        {
            UrlPath = Preferences.Get("url_temperature_table", "");
        }
    }
}