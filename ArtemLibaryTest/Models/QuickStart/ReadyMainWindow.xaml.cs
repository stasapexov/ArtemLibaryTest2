using ArtemLibaryTest.Core;
using ArtemLibaryTest.Models.QuickStart;
using System.Windows;

namespace ArtemLibaryTest.QuickStart
{
    public partial class ReadyMainWindow : Window
    {
        private readonly AuthUiContext _context;

        internal ReadyMainWindow(AuthUiContext context)
        {
            InitializeComponent();
            _context = context;
            Title = $"{_context.Options.AppTitle} - Главное меню";
            WelcomeText.Text = _context.Options.MainWelcomeText;
            UserIdLabel.Content = $"ID пользователя: {Session.CurrentUser?.Id}";
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Session.Logout();
            new ReadyLoginWindow(_context).Show();
            Close();
        }
    }
}