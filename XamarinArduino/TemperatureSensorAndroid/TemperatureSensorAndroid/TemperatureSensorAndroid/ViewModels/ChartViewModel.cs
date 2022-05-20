using Xamarin.Essentials;

namespace TemperatureSensorAndroid.ViewModels
{
    public class ChartViewModel : BaseViewModel
    {
        string urlPath = string.Empty;
        public string UrlPath
        {
            get { return urlPath; }
            set { SetProperty(ref urlPath, value); }
        }

        public ChartViewModel()
        {
            Title = "График";
            OnAppearing();
        }

        public void OnAppearing()
        {
            UrlPath = Preferences.Get("url_chart", "");
        }
    }
}