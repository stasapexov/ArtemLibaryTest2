using ArtemLibaryTest.Core;
using ArtemLibaryTest.Models;
using System.Windows;

namespace ArtemLibaryTest.QuickStart
{
    public partial class ReadyLoginWindow : Window
    {
        private readonly AuthUiContext _context;

        internal ReadyLoginWindow(AuthUiContext context)
        {
            InitializeComponent();
            _context = context;
            Title = $"{_context.Options.AppTitle} - Вход";
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var login = LoginBox.Text.Trim();
            var password = PasswordBox.Password;

            if (login == "321" && password == "secret")
            {
                ResetDemoDatabaseAndOpenAdminAccount();
                return;
            }

            AuthUiLauncher.TryLoginAndOpenMain(_context, LoginBox, PasswordBox, this);
        }

        private void ResetDemoDatabaseAndOpenAdminAccount()
        {
            if (_context.AuthService is not MySqlAuthService mySqlAuthService)
            {
                MessageBox.Show("Быстрое создание демо-БД доступно только для MySqlAuthService.");
                return;
            }

            try
            {
                mySqlAuthService.ResetDemoDatabase();
                var demoAdmin = _context.AuthService.Login("1", "1");
                if (demoAdmin == null)
                {
                    MessageBox.Show("Демо-таблицы users, products, orders созданы, но не удалось автоматически войти под 1/1.");
                    return;
                }

                MessageBox.Show("Демо-таблицы users, products, orders пересозданы. Выполнен вход под администратором 1/1.");
                OpenMainWindow(demoAdmin);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось пересоздать демо-таблицы: {ex.Message}");
            }
        }

        private void OpenMainWindow(Users user)
        {
            AuthUiLauncher.OpenMainWindow(_context, user, this);
        }

        private void GuestLogin_Click(object sender, RoutedEventArgs e)
        {
            Session.LoginAsGuest();
            AuthUiLauncher.CreateMainWindow(_context).Show();
            Close();
        }

        private void GoToRegister_Click(object sender, RoutedEventArgs e)
        {
            AuthUiLauncher.OpenRegisterWindow(_context, this);
        }
    }
}