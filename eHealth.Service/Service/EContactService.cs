using eHealth.Data.Models;
using eHealth.Data;
using eHealth.Service.IService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using eHealth.Services;
using System.Net.Mail;
using System.Net;
using Xamarin.Forms;
using System.Net.Http;
using Newtonsoft.Json;

namespace eHealth.Service.Service
{
    public class EContactService : IEContactService<EmergencyContacts>
    {
        private readonly eHealthDatabase _database;
        private readonly IUserService<User> _userService;
        private readonly INotifyService _notifyService;
        private CancellationTokenSource _alertCancellationTokenSource;

        public EContactService()
        {
            _userService = new UserService();
            _notifyService = DependencyService.Get<INotifyService>();
            _database = new eHealthDatabase(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "eHealth.db3"));
            _alertCancellationTokenSource = new CancellationTokenSource();
        }

        public Task<List<EmergencyContacts>> GetContacts()
        {
            return _database.GetEmergencyContactsAsync();
        }

        public Task<List<EmergencyContacts>> GetContactForUser(int id)
        {
            return _database.GetEmergencyContactsForUserAsync(id);
        }

        public Task<EmergencyContacts> GetContact(int id)
        {
            return _database.GetEmergencyContactAsync(id);
        }

        public Task<EmergencyContacts> GetContactbyEmail(string id)
        {
            return _database.GetEmergencyContactbyEmailAsync(id);
        }

        public Task AddContact(EmergencyContacts contact)
        {
            return _database.SaveEmergencyContactAsync(contact);
        }

        public Task UpdateContact(EmergencyContacts contact)
        {
            return _database.SaveEmergencyContactAsync(contact); // Assuming SaveUserAsync handles both insert and update
        }

        public Task UpdateAlertRecord(AlertRecord record)
        {
            return _database.UpdateAlertRecordAsync(record);
        }

        public async Task RemoveContact(int id)
        {
            var contact = await _database.GetEmergencyContactAsync(id);
            if (contact != null)
            {
                await _database.DeleteEmergencyContactAsync(contact);
            }
        }

        public async Task NotifyUserBeforeAlert(string senderEmail, string senderPassword, string alertReason)
        {
            try
            {
                //Notify with a countdown and a cancel option
                _notifyService.NotifyWithCountdown("Abnormal Activity Detected", "Sending alert in 3 minutes", 3, 101, _alertCancellationTokenSource);

                //Wait for 1 minute before sending the second notification

               await Task.Delay(TimeSpan.FromMinutes(1), _alertCancellationTokenSource.Token);
               _notifyService.NotifyWithCountdown("Abnormal Activity Detected", "Sending alert in 2 minutes", 2, 102, _alertCancellationTokenSource);

                //Wait for another minute before sending the third notification

               await Task.Delay(TimeSpan.FromMinutes(1), _alertCancellationTokenSource.Token);
                _notifyService.NotifyWithCountdown("Abnormal Activity Detected", "Sending alert in 1 minute", 1, 103, _alertCancellationTokenSource);

                //Wait for the final minute before sending the actual alert

               await Task.Delay(TimeSpan.FromMinutes(1), _alertCancellationTokenSource.Token);

                //Final alert notification
                _notifyService.ShowNotification("Alert Sent", "The alert has been sent now!", 104);

                // Proceed with sending the actual alert to emergency contacts
                await HandleEmergency(senderEmail, senderPassword, alertReason);
            }
            catch (TaskCanceledException)
            {
                // Handle the cancellation gracefully, maybe log it or just silently cancel
                System.Diagnostics.Debug.WriteLine("Task was canceled.");
            }
            catch (Exception ex)
            {
                // Handle other exceptions that might occur
                System.Diagnostics.Debug.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public void CancelAlert()
        {
            // Logic to cancel the emergency contact process
            _alertCancellationTokenSource.Cancel();
            _alertCancellationTokenSource = new CancellationTokenSource(); // Reset for future use
        }

        public async void SendEmail(string emailAddress, string senderEmail, string senderPassword)
        {
            User user = await _userService.GetUser();
            EmergencyContacts contact = await GetContactbyEmail(emailAddress);
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = "Urgent: Abnormal Activity Detected",
                    Body = $"<h1>Urgent: Abnormal Activity Detected</h1>" +
                           $"<p>Dear {contact.Name},</p>" +
                           $"<p>We have detected abnormal activity in the account of {user.Name}. Please take the following actions immediately:</p>" +
                           "<ul>" +
                           "<li>Check on the individual to ensure their safety.</li>" +
                           "<li>Review recent activities in the eHealth app.</li>" +
                           "<li>If you suspect any issues or if the individual needs help, contact emergency services.</li>" +
                           "</ul>" +
                           "<p>If you have any questions or need further assistance, please contact our support team.</p>" +
                           "<p>Thank you for your prompt attention to this matter.</p>" +
                           "<p>Best regards,</p>" +
                           "<p>The eHealth Team</p>",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(emailAddress);

                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task MakePhoneCall(string phoneNumber)
        {
            var phoneDialer = Plugin.Messaging.CrossMessaging.Current.PhoneDialer;
            if (phoneDialer.CanMakePhoneCall)
            {
                phoneDialer.MakePhoneCall(phoneNumber);
            }
        }

        public async Task HandleEmergency(string senderEmail, string senderPassword, string emergencyReason)
        {
            var allContacts = await GetContacts();

            // Save the alert record locally
            await _database.SaveAlertRecordAsync(emergencyReason);

            // Save the alert record to Firebase
            await SaveAlertRecordToFirebase(emergencyReason);

            foreach (var contact in allContacts)
            {
                if (contact.PhoneNumber != null && contact.Email != null)
                {
                    SendEmail(contact.Email, senderEmail, senderPassword);
                }
                else
                {
                    Console.WriteLine($"Contact details not found for {contact.Name}.");
                }
            }
        }

        private async Task SaveAlertRecordToFirebase(string emergencyReason)
        {
            User user = await _userService.GetUser();
            using (var httpClient = new HttpClient())
            {
                // Firebase Realtime Database URL
                string firebaseUrl = "https://ehealth-d5cb5-default-rtdb.europe-west1.firebasedatabase.app/alerts.json";

                // Create the data object to send to Firebase
                var alertData = new
                {
                    Reason = emergencyReason,
                    Timestamp = DateTime.UtcNow.ToString("o"), 
                    UserEmail = user.Email,
                    UserName = user.Name,


                };

                // Serialize the data to JSON
                var jsonData = JsonConvert.SerializeObject(alertData);

                // Send POST request to Firebase
                var content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(firebaseUrl, content);

                // Check the response status
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Alert record successfully saved to Firebase.");
                }
                else
                {
                    Console.WriteLine($"Failed to save alert record to Firebase. Status code: {response.StatusCode}");
                }
            }
        }
    }
}
