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

            var user = _context.AuthService.Login(login, password);
            if (user == null)
            {
                MessageBox.Show("Неверный логин или пароль");
                return;
            }

            OpenMainWindow(user);
        }

        private void OpenMainWindow(Users user)
        {
            Session.CurrentUser = user;
            new ReadyMainWindow(_context).Show();
            Close();
        }

        private void GuestLogin_Click(object sender, RoutedEventArgs e)
        {
            Session.LoginAsGuest();
            new ReadyMainWindow(_context).Show();
            Close();
        }

        private void GoToRegister_Click(object sender, RoutedEventArgs e)
        {
            new ReadyRegisterWindow(_context).Show();
            Close();
        }
    }
}