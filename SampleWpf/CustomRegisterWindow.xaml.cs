using System.Windows;
using ArtemLibaryTest.QuickStart;

namespace SampleWpf
{
    public partial class CustomRegisterWindow : Window
    {
        private readonly AuthUiContext _context;

        public CustomRegisterWindow(AuthUiContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            AuthUiLauncher.TryRegisterAndOpenLogin(
                _context,
                LoginBox,
                PasswordBox,
                NameBox,
                PhoneBox,
                EmailBox,
                this);
        }

        private void OpenLogin_Click(object sender, RoutedEventArgs e)
        {
            AuthUiLauncher.OpenLoginWindow(_context, this);
        }
    }
}
