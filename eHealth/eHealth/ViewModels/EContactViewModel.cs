using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using eHealth.Data.Models;
using eHealth.Service.IService;
using eHealth.Service.Service;
using eHealth.Views;
using Xamarin.Forms;

namespace eHealth.ViewModels
{
    public class EContactViewModel : BaseViewModel
    {
        private EmergencyContacts _selectedContact;
        private readonly IEContactService<EmergencyContacts> eContactService;

        public ObservableCollection<EmergencyContacts> Contacts { get; }
        public Command LoadContactsCommand { get; }
        public Command AddContactCommand { get; }
        public Command<EmergencyContacts> ContactTapped { get; }
        public Command<EmergencyContacts> DeleteContactCommand { get; }

        public EContactViewModel()
        {
            eContactService = new EContactService();

            Title = "Emergency Contacts";
            Contacts = new ObservableCollection<EmergencyContacts>();
            LoadContactsCommand = new Command(async () => await ExecuteLoadContactsCommand());

            AddContactCommand = new Command(OnAddContact);
            ContactTapped = new Command<EmergencyContacts>(OnContactSelected);
            DeleteContactCommand = new Command<EmergencyContacts>(OnDeleteContact);
        }

        async Task ExecuteLoadContactsCommand()
        {
            IsBusy = true;

            try
            {
                var allContacts = await eContactService.GetContacts();
                Device.BeginInvokeOnMainThread(() =>
                {
                    Contacts.Clear();
                    foreach (var contact in allContacts)
                    {
                        Contacts.Add(contact);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load contacts: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Error", "Failed to load contacts. Please try again.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

    
        public void OnAppearing()
        {
            IsBusy = true;
            SelectedContact = null;
            // Ensure contacts are reloaded when the view appears
            LoadContactsCommand.Execute(null);
        }

        public EmergencyContacts SelectedContact
        {
            get => _selectedContact;
            set => SetProperty(ref _selectedContact, value);
        }

        private async void OnAddContact(object obj)
        {
            await Shell.Current.GoToAsync(nameof(AddEmergencyContactPage));
        }

        private async void OnContactSelected(EmergencyContacts contact)
        {
            if (contact == null) return;

            SelectedContact = contact;
            await Shell.Current.GoToAsync($"{nameof(EContactDetailPage)}?ContactId={SelectedContact.ContactId}");
        }

        private async void OnDeleteContact(EmergencyContacts contact)
        {
            if (contact == null) return;

            var confirm = await App.Current.MainPage.DisplayAlert("Confirm Delete", "Are you sure you want to delete this contact?", "Yes", "No");
            if (!confirm) return;

            try
            {
                await eContactService.RemoveContact(contact.ContactId);
                Device.BeginInvokeOnMainThread(() => Contacts.Remove(contact));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to delete contact: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Error", "Failed to delete contact. Please try again.", "OK");
            }
        }
    }
}
