using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using eHealth.Data.Models;
using eHealth.Service.IService;
using eHealth.Services;

namespace eHealth.ViewModels
{
    public class AddUserDetailsViewModel : BaseViewModel
    {
        private string _name;
        private DateTime _dob;
        private string _gender;
        private string _email;
        private string _phoneNumber;
        private readonly IUserService<User> _userService;

        public AddUserDetailsViewModel()
        {
            _userService = new UserService();
            SaveCommand = new Command(OnSave, ValidateSave);
            CancelCommand = new Command(OnCancel);
            this.PropertyChanged += (_, __) => SaveCommand.ChangeCanExecute();
            DOB = DateTime.Today; // Set initial value of DOB
            LoadUserDetails();
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public DateTime DOB
        {
            get => _dob;
            set => SetProperty(ref _dob, value);
        }

        public string Gender
        {
            get => _gender;
            set => SetProperty(ref _gender, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        public Command SaveCommand { get; }
        public Command CancelCommand { get; }

        private bool ValidateSave()
        {
            return !String.IsNullOrWhiteSpace(Name)
                && DOB <= DateTime.Today
                && !String.IsNullOrWhiteSpace(Gender)
                && !String.IsNullOrWhiteSpace(Email)
                && !String.IsNullOrWhiteSpace(PhoneNumber);
        }

        private async void LoadUserDetails()
        {
            var user = await _userService.GetUser();
            if (user != null)
            {
                Name = user.Name;
                DOB = user.DOB;
                Gender = user.Gender;
                Email = user.Email;
                PhoneNumber = user.PhoneNumber;
            }
        }

        private async void OnSave()
        {
            var user = new User
            {
                Name = Name,
                DOB = DOB,
                Gender = Gender,
                Email = Email,
                PhoneNumber = PhoneNumber,
                age = CalculateAge(DOB) // Calculate and save age
            };

            var existingUser = await _userService.GetUser();
            if (existingUser == null)
            {
                await _userService.AddUser(user);
            }
            else
            {
                user.UserId = existingUser.UserId; // Ensure the correct UserId is set for update
                await _userService.UpdateUser(user);
            }

            await Shell.Current.GoToAsync("..");
        }

        private int CalculateAge(DateTime dob)
        {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;

            if (dob.Date > today.AddYears(-age)) age--;

            return age;
        }

        private async void OnCancel()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
