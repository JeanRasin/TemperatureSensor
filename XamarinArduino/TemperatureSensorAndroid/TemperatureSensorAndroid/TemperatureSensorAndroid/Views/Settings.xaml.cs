using TemperatureSensorAndroid.ViewModels;
using Xamarin.Forms;

namespace TemperatureSensorAndroid.Views
{
    public partial class Settings : ContentPage
    {
        public Settings()
        {
            InitializeComponent();
              BindingContext = new SettingsViewModel();
        }

    }
}