using System.Windows;
using System.Windows.Controls;
using ArtemLibaryTest.Core;

namespace SampleWpf;

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
        const string sql = @"
SELECT id, date, product_name, product_material, product_color, product_dimensions,
       quantity, unit_price, total_price, readiness
FROM orders
WHERE user_id = @userId
ORDER BY date DESC, id DESC;";
        OrdersGrid.ItemsSource = _db.GetTable(sql, DbHelper.Param("@userId", Session.CurrentUser?.Id ?? 0)).DefaultView;
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadOrders();
    }
}
