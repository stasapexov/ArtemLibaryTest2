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
SELECT order_id, order_date, product_name, quantity, total_price, readiness, pickup_address, pickup_code
FROM v_order_details
WHERE user_id = @user_id
ORDER BY order_id DESC",
            DbHelper.Param("@user_id", Session.CurrentUser.Id));

        OrdersGrid.ItemsSource = table.DefaultView;
        InfoTextBlock.Text = $"Заказов: {table.Rows.Count}";
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadOrders();
    }
}
