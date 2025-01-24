using System.Collections.Generic;
using System.Threading.Tasks;
using eHealth.Data.Models;
using eHealth.Data;
using eHealth.Service.IService;
using System.IO;
using System;

namespace eHealth.Services
{
    public class UserService : IUserService<User>
    {
        private readonly eHealthDatabase _database;

        public UserService()
        {
            _database = new eHealthDatabase(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "eHealth.db3"));
        }

        public Task<List<User>> GetUsers()
        {
            return _database.GetUsersAsync();
        }

        public Task<User> GetUser()
        {
            return _database.GetUserAsync();
        }

        public Task AddUser(User user)
        {
            return _database.SaveUserAsync(user);
        }

        public Task UpdateUser(User user)
        {
            return _database.SaveUserAsync(user); // Assuming SaveUserAsync handles both insert and update
        }
       


    }
}
