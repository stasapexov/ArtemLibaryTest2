using ArtemLibaryTest.Models;

namespace ArtemLibaryTest.Core
{
    public interface IAuthService
    {
        Users? Login(string login, string password);
        bool Register(string login, string password, string name, string phone);
    }
}
