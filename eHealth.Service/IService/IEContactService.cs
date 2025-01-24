using eHealth.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eHealth.Service.IService
{
    public interface IEContactService<T>
    {
        Task<List<EmergencyContacts>> GetContacts();
        Task<EmergencyContacts> GetContact(int id);
        Task AddContact(T contact);
        Task UpdateContact(T contact);

        Task UpdateAlertRecord(AlertRecord record);
        Task RemoveContact(int id);
        Task HandleEmergency(string senderEmail, string senderPassword, string emergencyReason);
        Task MakePhoneCall(string phoneNumber);

        Task NotifyUserBeforeAlert(string senderEmail, string senderPassword, string alertReason);
        void CancelAlert();



    }
}
