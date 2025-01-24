using eHealth.ViewModels;
using System;
using System.Timers;
using Xamarin.Forms;

namespace eHealth.Views
{
    public partial class UserPage : ContentPage
    {
        private Timer _refreshTimer;

        public UserPage()
        {
            InitializeComponent();
            BindingContext = new UserViewModel();

            _refreshTimer = new Timer(30000);
            _refreshTimer.Elapsed += OnRefreshTimerElapsed;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _refreshTimer.Start();
            try
            {
                ((UserViewModel)BindingContext)?.LoadSensorDataCommand.Execute(null);
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine($"Error during OnAppearing: {ex.Message}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _refreshTimer.Stop();
        }

        private void OnRefreshTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    ((UserViewModel)BindingContext)?.LoadSensorDataCommand.Execute(null);
                }
                catch (Exception ex)
                {
                    // Handle or log the exception
                    Console.WriteLine($"Error during timer refresh: {ex.Message}");
                }
            });
        }
    }
}
