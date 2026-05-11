1.Добавьте в корень проекта AppMenuProvider.cs c кодом:
using ArtemLibaryTest.Models;
using ArtemLibaryTest.QuickStart;

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
                    CreatePage = () => new Pages.HamePage()
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


2.Добавьте DbConfig.cs:

public static class DbConfig
    {
        // Подставь свои значения из phpMyAdmin/MySQL
        public const string ConnectionString = "server=localhost;database=demo;user=root;password=;";
    }


3.В App.xaml.cs: 

using ArtemLibaryTest.Core;
using ArtemLibaryTest.QuickStart;

protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var authService = new MySqlAuthService(DbConfig.ConnectionString);
            var options = new AuthUiOptions
            {
                AppTitle = "Exam Demo",
                MainWelcomeText = "Готовое главное меню из библиотеки",
                IsSettingsVisible = true,
                MenuProvider = new AppMenuProvider()
            };

            var loginWindow = AuthUiLauncher.CreateLoginWindow(authService, options);
            loginWindow.Show();
        }

4.DbConfig:
Server=127.0.0.1;Port=3306;Database=exam_demo;Uid=root;Pwd=;SslMode=None;Protocol=Tcp;
Вариант Б (если MySQL 8 ругается на auth key):
Server=127.0.0.1;Port=3306;Database=exam_demo;Uid=root;Pwd=;SslMode=None;Protocol=Tcp;AllowPublicKeyRetrieval=True;