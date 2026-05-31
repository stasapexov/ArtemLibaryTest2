using System.Windows;
using System.Windows.Controls;
using ArtemLibaryTest.Core;

namespace SampleWpf.Pages;

public partial class MyOrdersPage : Page
{
    private readonly DbHelper _db = new(DbConfig.ConnectionString);

    public MyOrdersPage()
    {
        InitializeComponent();
        LoadOrders();
    }

    private void LoadOrders()
    {
        if (Session.CurrentUser == null || Session.IsGuest)
        {
            OrdersGrid.ItemsSource = null;
            InfoTextBlock.Text = "Войдите, чтобы видеть свои заказы.";
            return;
        }

        var table = _db.GetTable(@"
SELECT
    o.id AS order_id,
    o.date AS order_date,
    o.product_name,
    o.quantity,
    o.total_price,
    o.readiness,
    o.pickup_address,
    o.pickup_code
FROM orders o
WHERE o.user_id = @user_id
ORDER BY o.id DESC",
            DbHelper.Param("@user_id", Session.CurrentUser.Id));

        OrdersGrid.ItemsSource = table.DefaultView;
        InfoTextBlock.Text = $"Заказов: {table.Rows.Count}";
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadOrders();
    }
}
