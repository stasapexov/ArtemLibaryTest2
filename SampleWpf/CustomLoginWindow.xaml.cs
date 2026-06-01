using System.Windows;
using ArtemLibaryTest.QuickStart;

namespace SampleWpf
{
    public partial class CustomLoginWindow : Window
    {
        private readonly AuthUiContext _context;

        public CustomLoginWindow(AuthUiContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            AuthUiLauncher.TryLoginAndOpenMain(_context, LoginBox, PasswordBox, this);
        }

        private void OpenRegister_Click(object sender, RoutedEventArgs e)
        {
            AuthUiLauncher.OpenRegisterWindow(_context, this);
        }
    }
}
