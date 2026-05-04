using System.Windows;

namespace ArtemLibaryTest.QuickStart
{
    public partial class UserProfileWindow : Window
    {
        public UserProfileWindow(AuthUiContext context)
        {
            InitializeComponent();
            ProfileFrame.Navigate(new UserProfilePage(context));
        }
    }
}
