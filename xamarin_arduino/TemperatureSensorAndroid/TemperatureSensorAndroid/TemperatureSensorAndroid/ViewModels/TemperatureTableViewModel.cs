using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TemperatureSensorAndroid.ViewModels
{
    public class TemperatureTableViewModel : BaseViewModel
    {
        public TemperatureTableViewModel()
        {
            Title = "Таблица данных";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamarin-quickstart"));
        }

        public ICommand OpenWebCommand { get; }
    }
}