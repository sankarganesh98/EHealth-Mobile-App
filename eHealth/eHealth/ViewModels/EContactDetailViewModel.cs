using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using eHealth.Service.IService;
using eHealth.Service.Service;
using eHealth.Data.Models;
using Xamarin.Essentials;

namespace eHealth.ViewModels
{
    [QueryProperty(nameof(ContactId), nameof(ContactId))]
    public class EContactDetailViewModel : BaseViewModel
    {
        private int contactId;
        private string name;
        private string relationship;
        private string email;
        private string phoneNumber;
        private bool isEditing;
        private string toolbarButtonText;
        private Command toolbarButtonCommand;

        IEContactService<EmergencyContacts> econtactService = new EContactService();

        public EContactDetailViewModel()
        {
            MakeCallCommand = new Command(async () => await MakeCall());
            EditCommand = new Command(OnEdit);
            SaveCommand = new Command(async () => await OnSave());
            ToolbarButtonText = "Edit";
            ToolbarButtonCommand = EditCommand;
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

        public int ContactId
        {
            get => contactId;
            set
            {
                contactId = value;
                LoadContactId(value);
            }
        }

        public bool IsEditing
        {
            get => isEditing;
            set
            {
                SetProperty(ref isEditing, value);
                ToolbarButtonText = isEditing ? "Save" : "Edit";
                ToolbarButtonCommand = isEditing ? SaveCommand : EditCommand;
            }
        }

        public string ToolbarButtonText
        {
            get => toolbarButtonText;
            set => SetProperty(ref toolbarButtonText, value);
        }

        public Command ToolbarButtonCommand
        {
            get => toolbarButtonCommand;
            set => SetProperty(ref toolbarButtonCommand, value);
        }

        public Command HandleEmergencyCommand { get; }

        public Command MakeCallCommand { get; }
        public Command EditCommand { get; }
        public Command SaveCommand { get; }

        public async void LoadContactId(int contactId)
        {
            try
            {
                var contact = await econtactService.GetContact(contactId);
                Name = contact.Name;
                Relationship = contact.Relationship;
                Email = contact.Email;
                PhoneNumber = contact.PhoneNumber;
                IsEditing = false; // Start in read-only mode
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to Load Item");
            }
        }

        
        private async Task MakeCall()
        {
            var contact = await econtactService.GetContact(contactId);
            PhoneNumber = contact.PhoneNumber;
            await econtactService.MakePhoneCall(PhoneNumber);


        }

        private void OnEdit()
        {
            IsEditing = true;
        }

        private async Task OnSave()
        {
            var contact = new EmergencyContacts
            {
                ContactId = ContactId,
                Name = Name,
                Relationship = Relationship,
                Email = Email,
                PhoneNumber = PhoneNumber
            };

            try
            {
                await econtactService.UpdateContact(contact);
                IsEditing = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save contact: {ex.Message}");
                await App.Current.MainPage.DisplayAlert("Error", "Failed to save contact. Please try again.", "OK");
            }
        }
    }
}
