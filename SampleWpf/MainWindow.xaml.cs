using System.Windows;

namespace SampleWpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CatalogFrame.Navigate(new ProductOrderPage());
            OrdersFrame.Navigate(new MyOrdersPage());
        }
    }
}
