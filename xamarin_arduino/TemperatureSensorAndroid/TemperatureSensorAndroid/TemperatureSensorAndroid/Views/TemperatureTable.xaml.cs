using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TemperatureSensorAndroid.ViewModels;
using TemperatureSensorAndroid.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TemperatureSensorAndroid.Views
{
    public partial class TemperatureTable : ContentPage
    {
        //ItemsViewModel _viewModel;

        public TemperatureTable()
        {
            InitializeComponent();

           // BindingContext = _viewModel = new ItemsViewModel();
        }

        //protected override void OnAppearing()
        //{
        //    base.OnAppearing();
        //    _viewModel.OnAppearing();
        //}
    }
}