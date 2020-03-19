using System.Threading.Tasks;
using DatingApp.WebAPI.Models;

namespace DatingApp.WebAPI.Context
{
    public interface IAuthRepository
    {
        //Register user
        Task<User> Register(User user, string password);
        //log at the api
        Task<User> Login(string username, string password);

        //check if exists
        Task<bool> UserExists(string username);
    }
}