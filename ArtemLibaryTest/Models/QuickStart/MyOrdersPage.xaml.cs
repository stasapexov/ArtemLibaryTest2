using ArtemLibaryTest.Core;
using System.Windows.Controls;

namespace ArtemLibaryTest.QuickStart
{
    public partial class MyOrdersPage : Page
    {
        private readonly AuthUiContext _context;

        public MyOrdersPage(AuthUiContext context)
        {
            InitializeComponent();
            _context = context;
            LoadOrders();
        }

        private void LoadOrders()
        {
            if (_context.AuthService is not MySqlAuthService mysql || Session.CurrentUser == null)
            {
                return;
            }

            OrdersGrid.ItemsSource = mysql.GetMyOrders(Session.CurrentUser.Id).DefaultView;
        }
    }
}
