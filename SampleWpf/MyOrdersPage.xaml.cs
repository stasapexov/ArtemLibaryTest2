using ArtemLibaryTest.Core;
using System.Windows.Controls;

namespace SampleWpf;

public partial class MyOrdersPage : Page
{
    private readonly DbHelper _db = new(DbConfig.ConnectionString);
    public MyOrdersPage()
    {
        InitializeComponent();
        var userId = Session.CurrentUser?.Id ?? 49;
        OrdersItems.ItemsSource = _db.GetTable(@"SELECT o.id AS order_id, p.name AS material_name, oi.quantity, oi.unit_price, o.total_price, o.readiness FROM orders o JOIN order_items oi ON oi.order_id=o.id JOIN products p ON p.id=oi.product_id WHERE o.user_id=@uid ORDER BY o.id DESC;", DbHelper.Param("@uid", userId)).DefaultView;
    }
}
