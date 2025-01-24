using SQLite;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using eHealth.Data.Models;
using System;

namespace eHealth.Data
{
    public class eHealthDatabase
    {
        private readonly SQLiteAsyncConnection _database;

        public eHealthDatabase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
        }

        public async Task InitializeAsync()
        {
            await CreateTablesAsync();
        }

        private async Task CreateTablesAsync()
        {
            // Create tables with the correct schema
            await _database.CreateTableAsync<User>();
            await _database.CreateTableAsync<SensorData>();
            await _database.CreateTableAsync<EmergencyContacts>();
            await _database.CreateTableAsync<AlertRecord>();
        }

        public static string FormatDateTime(long ticks)
        {
            DateTime dateTime = new DateTime(ticks);
            return dateTime.ToString("dd:MM:yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        }

        // User CRUD operations
        public Task<List<User>> GetUsersAsync()
        {
            return _database.Table<User>().ToListAsync();
        }

        public Task<User> GetUserAsync()
        {
            return _database.Table<User>().FirstOrDefaultAsync();
        }

        public Task<int> SaveUserAsync(User user)
        {
            if (user.UserId != 0)
            {
                return _database.UpdateAsync(user);
            }
            else
            {
                return _database.InsertAsync(user);
            }
        }

        public Task<int> DeleteUserAsync(User user)
        {
            return _database.DeleteAsync(user);
        }

        // SensorData CRUD operations
        public Task<SensorData> GetSensorAsync(DateTime date)
        {
            return _database.Table<SensorData>()
                            .Where(i => i.DateTime == date)
                            .FirstOrDefaultAsync();
        }

        public async Task<int> SaveSensorDataAsync(SensorData sensorData)
        {
            User user = await GetUserAsync();
            if (user != null)
            {
                sensorData.UserEmail = user.Email;
            }
            return await _database.InsertAsync(sensorData);
        }

        public async Task DeleteOldSensorDataAsync()
        {
            var thresholdDate = DateTime.Now.AddDays(-15);
            var oldData = await _database.Table<SensorData>().Where(data => data.DateTime < thresholdDate).ToListAsync();
            foreach (var data in oldData)
            {
                await _database.DeleteAsync(data);
            }
        }

        public Task<List<SensorData>> GetAllSensorDataAsync()
        {
            return _database.Table<SensorData>().ToListAsync();
        }

        public Task<int> DeleteSensorAsync(SensorData sensor)
        {
            return _database.DeleteAsync(sensor);
        }

        public Task<int> DeleteAllSensorDataAsync()
        {
            return _database.DeleteAllAsync<SensorData>();
        }

        // EmergencyContacts CRUD operations
        public Task<List<EmergencyContacts>> GetEmergencyContactsAsync()
        {
            return _database.Table<EmergencyContacts>().ToListAsync();
        }

        public Task<List<EmergencyContacts>> GetEmergencyContactsForUserAsync(int userId)
        {
            return _database.Table<EmergencyContacts>()
                           .Where(i => i.UserId == userId)
                           .ToListAsync();
        }

        public Task<EmergencyContacts> GetEmergencyContactAsync(int id)
        {
            return _database.Table<EmergencyContacts>()
                            .Where(i => i.ContactId == id)
                            .FirstOrDefaultAsync();
        }

        public Task<EmergencyContacts> GetEmergencyContactbyEmailAsync(string email)
        {
            return _database.Table<EmergencyContacts>()
                            .Where(i => i.Email == email)
                            .FirstOrDefaultAsync();
        }

        public async Task<int> SaveEmergencyContactAsync(EmergencyContacts contact)
        {
            if (contact.ContactId != 0)
            {
                Console.WriteLine($"Updating Emergency Contact: {contact.ContactId}");
                var result = await _database.UpdateAsync(contact);
                Console.WriteLine($"Update result: {result}");
                return result;
            }
            else
            {
                Console.WriteLine("Inserting new Emergency Contact");
                User user = await GetUserAsync();
                if (user != null)
                {
                    contact.UserId = user.UserId;
                    contact.UserEmail = user.Email;
                }
                var result = await _database.InsertAsync(contact);
                Console.WriteLine($"Insert result: {result}");
                return result;
            }
        }

        public Task<int> DeleteEmergencyContactAsync(EmergencyContacts contact)
        {
            return _database.DeleteAsync(contact);
        }

        // AlertRecord CRUD operations
        public async Task<int> SaveAlertRecordAsync(string alertReason)
        {
            User user = await GetUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            var alertRecord = new AlertRecord
            {
                alert_DateTime = DateTime.Now,  // Record the current date and time
                userId = user.UserId,
                userEmail = user.Email,
                alertReason = alertReason
            };

            int localResult = await _database.InsertAsync(alertRecord);
            return localResult;
        }

        public async Task<int> UpdateAlertRecordAsync(AlertRecord record)
        {
            var result = await _database.UpdateAsync(record);
            return result;
        }

        public Task<List<AlertRecord>> GetAlertRecordsAsync()
        {
            return _database.Table<AlertRecord>().ToListAsync();
        }
    }
}
