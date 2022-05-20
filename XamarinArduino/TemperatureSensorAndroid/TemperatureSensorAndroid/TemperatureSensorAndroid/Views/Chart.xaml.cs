using TemperatureSensorAndroid.ViewModels;
using Xamarin.Forms;

namespace TemperatureSensorAndroid.Views
{
    public partial class Chart : ContentPage
    {
       private readonly ChartViewModel _viewModel;

        public Chart()
        {
            InitializeComponent();
            BindingContext = _viewModel = new ChartViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}