using eHealth.Data;
using eHealth.Service.Service;
using eHealth.ViewModels;
using eHealth.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace eHealth.Views
{
    public partial class EContactPage : ContentPage
    {
        
        EContactViewModel _viewModel;

        public EContactPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new EContactViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}