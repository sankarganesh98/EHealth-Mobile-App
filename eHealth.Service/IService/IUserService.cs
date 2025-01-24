using eHealth.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eHealth.Service.IService
{
    public interface IUserService<T>
    {
        Task<List<User>> GetUsers();
        Task<User> GetUser();
        Task AddUser(T user);
        Task UpdateUser(T user);
        
    }
}
