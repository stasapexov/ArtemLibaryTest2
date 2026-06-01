using System.Configuration;
using System.Data;
using System.Windows;
using ArtemLibaryTest;
using ArtemLibaryTest.Core;
using ArtemLibaryTest.QuickStart;
namespace SampleWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var authService = new MySqlAuthService(DbConfig.ConnectionString);
            var options = new AuthUiOptions
            {
                AppTitle = "Exam Demo",
                MainWelcomeText = "Готовое главное меню из библиотеки",
                LoginWindowFactory = context => new CustomLoginWindow(context),
                RegisterWindowFactory = context => new CustomRegisterWindow(context)
            };

            var loginWindow = AuthUiLauncher.CreateLoginWindow(authService, options);
            loginWindow.Show();
        }
    }

}
