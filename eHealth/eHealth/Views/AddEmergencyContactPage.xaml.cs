using eHealth.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using eHealth.Data.Models;
using eHealth.Data;
using eHealth.Service;

namespace eHealth.Views
{
    public partial class AddEmergencyContactPage : ContentPage
    {
        public EmergencyContacts EmergencyContacts { get; set; }

        public AddEmergencyContactPage()
        {
            InitializeComponent();
            BindingContext = new AddEmergencyContactViewModel();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var viewModel = BindingContext as AddEmergencyContactViewModel;
            viewModel.SaveCommand.Execute(viewModel);

        }
    }
}