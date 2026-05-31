using System.Text;
using System.Windows;
using System.Windows.Controls;
using ArtemLibaryTest.Core;
using MySql.Data.MySqlClient;

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

        var sql = new StringBuilder(@"
SELECT order_id, order_date, product_name, quantity, total_price, readiness, pickup_address, pickup_code
FROM v_user_orders
WHERE user_id = @user_id");
        var parameters = new List<MySqlParameter>
        {
            DbHelper.Param("@user_id", Session.CurrentUser.Id)
        };

        AddOrderFilters(sql, parameters);
        sql.Append(" ORDER BY order_id DESC");

        var table = _db.GetTable(sql.ToString(), parameters.ToArray());
        OrdersGrid.ItemsSource = table.DefaultView;
        InfoTextBlock.Text = $"Заказов: {table.Rows.Count}";
    }

    private void AddOrderFilters(StringBuilder sql, List<MySqlParameter> parameters)
    {
        DbHelper.AddWhereLikeAnyWord(sql, parameters, "product_name", "@product_name", ProductNameTextBox.Text);
        DbHelper.AddWhereMin(sql, parameters, "total_price", "@min_price", ParseNullableDouble(MinPriceTextBox.Text));
        DbHelper.AddWhereMax(sql, parameters, "total_price", "@max_price", ParseNullableDouble(MaxPriceTextBox.Text));

        if (DateTime.TryParse(DateTextBox.Text, out var date))
        {
            sql.Append(" AND order_date = @order_date");
            parameters.Add(DbHelper.Param("@order_date", date.Date));
        }
    }

    private void Filter_Click(object sender, RoutedEventArgs e)
    {
        LoadOrders();
    }

    private void ResetFilter_Click(object sender, RoutedEventArgs e)
    {
        ProductNameTextBox.Clear();
        MinPriceTextBox.Clear();
        MaxPriceTextBox.Clear();
        DateTextBox.Clear();
        LoadOrders();
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadOrders();
    }

    private static double? ParseNullableDouble(string text)
    {
        return double.TryParse(text, out var value) ? value : null;
    }
}
