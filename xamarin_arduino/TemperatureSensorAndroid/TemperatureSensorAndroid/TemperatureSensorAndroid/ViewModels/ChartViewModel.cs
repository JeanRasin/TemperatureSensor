using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TemperatureSensorAndroid.ViewModels
{
    public class ChartViewModel : BaseViewModel
    {
        public ChartViewModel()
        {
            Title = "График";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamarin-quickstart"));
        }

        public ICommand OpenWebCommand { get; }
    }
}