using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using eHealth.Data.Models;
using eHealth.Service.IService;
using eHealth.Service.Service;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace eHealth.ViewModels
{
    public class EmergencyViewModel : BaseViewModel
    {
        private readonly IEContactService<EmergencyContacts> _emergencyService;
        private string _senderEmail;
        private string _senderPassword;
        private string _emergencyStatusMessage;

        public ICommand EmergencyCommand { get; }

        public string EmergencyStatusMessage
        {
            get => _emergencyStatusMessage;
            set => SetProperty(ref _emergencyStatusMessage, value);
        }

        public EmergencyViewModel()
        {
            _emergencyService = new EContactService();
            EmergencyCommand = new Command(async () => await ExecuteEmergencyCommand());
            Title = "Emergency";
        }

        private async Task ExecuteEmergencyCommand()
        {
            IsBusy = true;

            try
            {
                _senderEmail = await SecureStorage.GetAsync("email");
                _senderPassword = await SecureStorage.GetAsync("password");

                if (string.IsNullOrEmpty(_senderEmail) || string.IsNullOrEmpty(_senderPassword))
                {
                    EmergencyStatusMessage = "Email or password not found in secure storage.";
                    Debug.WriteLine("Email or password not found in secure storage.");
                    throw new InvalidOperationException("Email or password not found in secure storage.");
                }

                Debug.WriteLine($"Handling emergency with email: {_senderEmail}");
                Debug.WriteLine($"Handling emergency with Password: {_senderPassword}");
                await _emergencyService.HandleEmergency(_senderEmail, _senderPassword, "User Pressed Emergency button");
                Debug.WriteLine("Emergency handled.");
                EmergencyStatusMessage = "Emergency triggered successfully.";
            }
            catch (Exception ex)
            {
                EmergencyStatusMessage = $"Failed to trigger emergency: {ex.Message}";
                Debug.WriteLine($"Failed to trigger emergency: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
