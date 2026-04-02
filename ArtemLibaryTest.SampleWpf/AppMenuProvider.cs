using ArtemLibaryTest.Models;
using ArtemLibaryTest.QuickStart;

namespace ArtemLibaryTest.SampleWpf
{
    public class AppMenuProvider : IMenuProvider
    {
        public IEnumerable<NavMenuItem> GetMenuItems(Users currentUser)
        {
            return
            [
                new NavMenuItem
                {
                    Title = "Главная",
                    Tag = "Home",
                    Icon = "Home",
                    Roles = ["admin", "user", "manager"],
                    CreatePage = () => new Pages.HomePage()
                },
                new NavMenuItem
                {
                    Title = "Профиль",
                    Tag = "Profile",
                    Icon = "Contact",
                    Roles = ["admin", "user", "manager"],
                    CreatePage = () => new Pages.ProfilePage()
                },
                new NavMenuItem
                {
                    Title = "Админ панель",
                    Tag = "Admin",
                    Icon = "Admin",
                    Roles = ["admin"],
                    CreatePage = () => new Pages.AdminPage()
                }
            ];
        }
    }
}
