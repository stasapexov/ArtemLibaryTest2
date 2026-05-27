using System.Windows;
using System.Windows.Controls;

namespace SampleWpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            OpenShop();
        }

        private void OpenShop_Click(object sender, RoutedEventArgs e) => OpenShop();

        private void OpenShop()
        {
            var role = (RoleBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "guest";
            var isAuthorized = role != "guest";
            MainFrame.Navigate(new ShopPage(role, isAuthorized));
        }
    }
}
