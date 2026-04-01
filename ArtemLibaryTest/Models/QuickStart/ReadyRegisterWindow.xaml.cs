using System.Windows;

namespace ArtemLibaryTest.QuickStart
{
    public partial class ReadyRegisterWindow : Window
    {
        private readonly AuthUiContext _context;

        internal ReadyRegisterWindow(AuthUiContext context)
        {
            InitializeComponent();
            _context = context;
            Title = $"{_context.Options.AppTitle} - Регистрация";
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var success = _context.AuthService.Register(
                LoginBox.Text.Trim(),
                PasswordBox.Password,
                NameBox.Text.Trim(),
                PhoneBox.Text.Trim());

            if (!success)
            {
                MessageBox.Show("Логин уже занят");
                return;
            }

            MessageBox.Show("Регистрация успешна");
            new ReadyLoginWindow(_context).Show();
            Close();
        }
    }
}
