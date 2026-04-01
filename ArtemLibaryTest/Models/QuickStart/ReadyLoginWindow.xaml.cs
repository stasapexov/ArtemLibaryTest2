using ArtemLibaryTest.Core;
using ArtemLibaryTest.Models.QuickStart;
using System.Windows;
using System.Windows.Controls;

namespace ArtemLibaryTest.Models.QuickStart
{
    public partial class ReadyLoginWindow : Window
    {
        private readonly Core.IAuthService _context;

        internal ReadyLoginWindow(AuthUiContext context)
        {
            InitializeComponent();
            _context = context;
            Title = $"{_context.Options.AppTitle} - Вход";
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var user = _context.AuthService.Login(LoginBox.Text.Trim(), PasswordBox.Password);
            if (user == null)
            {
                MessageBox.Show("Неверный логин или пароль");
                return;
            }

            Session.CurrentUser = user;
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