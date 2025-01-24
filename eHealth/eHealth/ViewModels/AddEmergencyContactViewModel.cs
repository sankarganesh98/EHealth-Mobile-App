using eHealth.Data.Models;
using eHealth.Service.IService;
using eHealth.Service.Service;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace eHealth.ViewModels
{
    public class AddEmergencyContactViewModel : BaseViewModel
    {
        private string name;
        private string relationship;
        private string email;
        private string phoneNumber;
        IEContactService<EmergencyContacts> eContactService;

        public AddEmergencyContactViewModel()
        {
           eContactService = new EContactService();
            SaveCommand = new Command(OnSave, ValidateSave);
            CancelCommand = new Command(OnCancel);
            this.PropertyChanged +=
                (_, __) => SaveCommand.ChangeCanExecute();
        }

        private bool ValidateSave()
        {
            return !String.IsNullOrWhiteSpace(name)
                && !String.IsNullOrWhiteSpace(relationship)
            && !String.IsNullOrWhiteSpace(email)
            && !String.IsNullOrWhiteSpace(phoneNumber);
        }

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public string Relationship
        {
            get => relationship;
            set => SetProperty(ref relationship, value);
        }
        public string Email
        {
            get => email;
            set => SetProperty(ref email, value);
        }
        public string PhoneNumber
        {
            get => phoneNumber;
            set => SetProperty(ref phoneNumber, value);
        }

        public Command SaveCommand { get; }
        public Command CancelCommand { get; }

        private async void OnCancel()
        {
            // This will pop the current page off the navigation stack
            await Shell.Current.GoToAsync("..");
        }

        private async void OnSave()
        {
            EmergencyContacts contact = new EmergencyContacts()
            {
                Name = Name,
                Relationship = Relationship,
                Email = Email,
                PhoneNumber = PhoneNumber
            };

            await eContactService.AddContact(contact);


            // This will pop the current page off the navigation stack
            await Shell.Current.GoToAsync("..");
        }
    }
}
