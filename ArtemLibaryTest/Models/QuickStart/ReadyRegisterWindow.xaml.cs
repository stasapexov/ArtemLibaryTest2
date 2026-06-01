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
            AuthUiLauncher.TryRegisterAndOpenLogin(_context, LoginBox, PasswordBox, NameBox, PhoneBox, EmailBox, this);
        }
        private void GoToRegister_Click(object sender, RoutedEventArgs e)
        {
            AuthUiLauncher.OpenLoginWindow(_context, this);
        }
    }
}