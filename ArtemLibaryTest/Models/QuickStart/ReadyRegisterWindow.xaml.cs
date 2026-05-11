using ArtemLibaryTest.Models.QuickStart;
using System.Windows;
using System.Windows.Controls;

namespace ArtemLibaryTest.QuickStart
{
    public partial class ReadyRegisterWindow : Window
    {
        private readonly AuthUiContext _context;

        internal ReadyRegisterWindow(AuthUiContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var success = _context.AuthService.Register(
                LoginBox.Text.Trim(),
                PasswordBox.Password,
                NameBox.Text.Trim(),
                PhoneBox.Text.Trim(),
                EmailBox.Text.Trim());

            if (!success)
            {
                MessageBox.Show("Логин уже занят");
                return;
            }

            MessageBox.Show("Регистрация успешна");
            new ReadyLoginWindow(_context).Show();
            Close();
        }
        private void GoToRegister_Click(object sender, RoutedEventArgs e)
        {
            new ReadyLoginWindow(_context).Show();
            Close();
        }
    }
}