using System.Windows;
using ArtemLibaryTest.Core;
using ArtemLibaryTest.QuickStart;

namespace SampleWpf
{
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
                MenuProvider = new SampleMenuProvider()
            };

            var loginWindow = AuthUiLauncher.CreateLoginWindow(authService, options);
            loginWindow.Show();
        }
    }
}
