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

        public static void Logout()
        {
            CurrentUser = null;
        }

        public static bool IsLoggedIn => CurrentUser != null;
        public static bool IsAdmin => CurrentUser != null && CurrentUser.Status == "admin";
        public static bool IsManager => CurrentUser != null && CurrentUser.Status == "manager";
    }
}
