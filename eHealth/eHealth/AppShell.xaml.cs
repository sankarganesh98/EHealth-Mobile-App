using eHealth.ViewModels;
using eHealth.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace eHealth
{

    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(EContactDetailPage), typeof(EContactDetailPage));
            Routing.RegisterRoute(nameof(AddEmergencyContactPage), typeof(AddEmergencyContactPage));
            Routing.RegisterRoute(nameof(ModifyEContactDetailPage), typeof(ModifyEContactDetailPage));
            Routing.RegisterRoute(nameof(AddUserDetailsPage), typeof(AddUserDetailsPage));
        }

    }
}
