using ArtemLibaryTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemLibaryTest.Core
{
    public static class Session
    {
        public static Users? CurrentUser { get; set; }

        public static Users CreateGuestUser()
        {
            return new Users(0, "Гость", string.Empty, "guest", string.Empty, string.Empty, "guest", 0, []);
        }

        public static void LoginAsGuest()
        {
            CurrentUser = CreateGuestUser();
        }

        public static void Logout()
        {
            CurrentUser = null;
        }

        public static bool IsLoggedIn => CurrentUser != null;
        public static bool IsGuest => CurrentUser != null && CurrentUser.Status == "guest";
        public static bool IsAdmin => CurrentUser != null && CurrentUser.Status == "admin";
        public static bool IsManager => CurrentUser != null && CurrentUser.Status == "manager";
    }
}
