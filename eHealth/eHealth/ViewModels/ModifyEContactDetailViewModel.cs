using System;
using System.Diagnostics;
using System.Threading.Tasks;
using eHealth.Data.Models;
using eHealth.Service.IService;
using eHealth.Service.Service;
using Xamarin.Forms;

namespace eHealth.ViewModels
{
    [QueryProperty(nameof(ContactId), nameof(ContactId))]
    public class ModifyEContactDetailViewModel : BaseViewModel
    {
        private int contactId;
        private string name;
        private string relationship;
        private string email;
        private string phoneNumber;
        private readonly IEContactService<EmergencyContacts> eContactService;

        public ModifyEContactDetailViewModel()
        {
            eContactService = new EContactService();
            SaveCommand = new Command(OnSave, ValidateSave);
            CancelCommand = new Command(OnCancel);
            this.PropertyChanged += (_, __) => SaveCommand.ChangeCanExecute();
        }

        public int ContactId
        {
            get => contactId;
            set
            {
                contactId = value;
                LoadContactId(value);
            }
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

        private bool ValidateSave()
        {
            return !String.IsNullOrWhiteSpace(Name)
                && !String.IsNullOrWhiteSpace(Relationship)
                && !String.IsNullOrWhiteSpace(Email)
                && !String.IsNullOrWhiteSpace(PhoneNumber);
        }

        private async void LoadContactId(int contactId)
        {
            try
            {
                var contact = await eContactService.GetContact(contactId);
                if (contact != null)
                {
                    Name = contact.Name;
                    Relationship = contact.Relationship;
                    Email = contact.Email;
                    PhoneNumber = contact.PhoneNumber;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to Load Contact: " + ex.Message);
            }
        }

        private async void OnSave()
        {
            try
            {
                var contact = new EmergencyContacts
                {
                    ContactId = ContactId,
                    Name = Name,
                    Relationship = Relationship,
                    Email = Email,
                    PhoneNumber = PhoneNumber
                };

                await eContactService.UpdateContact(contact);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to Save Contact: " + ex.Message);
            }
        }

        private async void OnCancel()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
