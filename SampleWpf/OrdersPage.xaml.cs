using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ArtemLibaryTest.Core;

namespace SampleWpf;

public partial class OrdersPage : Page
{
    private readonly DbHelper _db = new(DbConfig.ConnectionString);

    public OrdersPage()
    {
        InitializeComponent();
        ConfigureRole();
        LoadOrders();
    }

    private bool IsAdmin => string.Equals(Session.CurrentUser?.Status, "admin", StringComparison.OrdinalIgnoreCase);

    private void ConfigureRole()
    {
        AdminFilterPanel.Visibility = IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        DeleteButton.IsEnabled = IsAdmin;
        RoleHintText.Text = IsAdmin
            ? "Администратор может фильтровать и удалять заказы."
            : "Менеджер может только просматривать заказы.";
    }

    private void LoadOrders()
    {
        var sql = new StringBuilder(@"
SELECT o.id, o.date, u.login AS user_login, u.name AS user_name,
       o.product_name, o.product_material, o.product_color, o.product_dimensions,
       o.quantity, o.unit_price, o.total_price, o.readiness
FROM orders o
INNER JOIN users u ON u.id = o.user_id
WHERE 1 = 1");
        var parameters = new List<MySql.Data.MySqlClient.MySqlParameter>();

        if (IsAdmin)
        {
            DbHelper.AddWhereLike(sql, parameters, "o.product_name", "@productName", ProductNameFilterBox.Text);
            DbHelper.AddWhereMin(sql, parameters, "o.total_price", "@minPrice", ReadDouble(MinPriceBox.Text));
            DbHelper.AddWhereMax(sql, parameters, "o.total_price", "@maxPrice", ReadDouble(MaxPriceBox.Text));
        }

        sql.Append(" ORDER BY o.date DESC, o.id DESC;");
        OrdersGrid.ItemsSource = _db.GetTable(sql.ToString(), parameters.ToArray()).DefaultView;
    }

    private static double? ReadDouble(string text)
    {
        return double.TryParse(text.Trim(), out var value) ? value : null;
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        LoadOrders();
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        ProductNameFilterBox.Clear();
        MinPriceBox.Clear();
        MaxPriceBox.Clear();
        LoadOrders();
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (!IsAdmin)
        {
            MessageBox.Show("Удалять заказы может только администратор.");
            return;
        }

        if (OrdersGrid.SelectedItem is not DataRowView row)
        {
            MessageBox.Show("Выберите заказ.");
            return;
        }

        var orderId = Convert.ToInt32(row["id"]);
        _db.ExecuteNonQuery("DELETE FROM orders WHERE id = @id;", DbHelper.Param("@id", orderId));
        LoadOrders();
    }
}
