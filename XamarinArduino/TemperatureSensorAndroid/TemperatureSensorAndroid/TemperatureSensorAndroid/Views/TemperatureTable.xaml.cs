using TemperatureSensorAndroid.ViewModels;
using Xamarin.Forms;

namespace TemperatureSensorAndroid.Views
{
    public partial class TemperatureTable : ContentPage
    {
        private readonly TemperatureTableViewModel _viewModel;

        public TemperatureTable()
        {
            InitializeComponent();
            BindingContext = _viewModel = new TemperatureTableViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}