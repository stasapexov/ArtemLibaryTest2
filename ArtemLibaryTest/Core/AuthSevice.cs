using ArtemLibaryTest.Models;

namespace ArtemLibaryTest.Core
{
    public interface IAuthService
    {
        Users? Login(string login, string password);
        bool Register(string login, string password, string name, string phone);
        bool UpdateProfile(int userId, string login, string password, string phone);
        bool UpdateAvatar(int userId, byte[] imageBytes);
    }
}
