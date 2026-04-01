using ArtemLibaryTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemLibaryTest.Core
{
    public interface IAuthService
    {
        Users? Login(string login, string password);
        bool Register(string login, string password, string name, string phone);
    }
}
