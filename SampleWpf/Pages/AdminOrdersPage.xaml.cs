using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ArtemLibaryTest.Core;
using MySql.Data.MySqlClient;

namespace SampleWpf.Pages;

public partial class AdminOrdersPage : Page
{
    private readonly DbHelper _db = new(DbConfig.ConnectionString);

    public AdminOrdersPage()
    {
        InitializeComponent();
        LoadOrders();
        LoadPickupPoints();
    }

    private void LoadOrders()
    {
        var sql = new StringBuilder(@"
SELECT order_id, order_date, user_login, product_name, quantity, total_price, readiness, pickup_address, pickup_code
FROM v_admin_orders
WHERE 1=1");
        var parameters = new List<MySqlParameter>();

        AddOrderFilters(sql, parameters);
        sql.Append(" ORDER BY order_id DESC");

        var table = _db.GetTable(sql.ToString(), parameters.ToArray());
        OrdersGrid.ItemsSource = table.DefaultView;
        OrdersInfoTextBlock.Text = $"Заказов: {table.Rows.Count}";
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

    private void LoadPickupPoints()
    {
        var table = _db.GetTable(@"
SELECT id, name, address, phone, working_hours
FROM pickup_points
ORDER BY address, name");
        PickupPointsGrid.ItemsSource = table.DefaultView;
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

    private void SaveStatus_Click(object sender, RoutedEventArgs e)
    {
        var orderId = GetSelectedOrderId();
        if (orderId == null)
        {
            MessageBox.Show("Выберите заказ.");
            return;
        }

        var status = (ReadinessComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "New";
        _db.ExecuteNonQuery(
            "UPDATE orders SET readiness = @readiness WHERE id = @order_id",
            DbHelper.Param("@readiness", status),
            DbHelper.Param("@order_id", orderId.Value));

        LoadOrders();
    }

    private void DeleteOrder_Click(object sender, RoutedEventArgs e)
    {
        var orderId = GetSelectedOrderId();
        if (orderId == null)
        {
            MessageBox.Show("Выберите заказ.");
            return;
        }

        if (MessageBox.Show("Удалить выбранный заказ?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
            return;
        }

        using var connection = new MySqlConnection(DbConfig.ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            ExecuteInTransaction(connection, transaction, @"
UPDATE products p
INNER JOIN orders o ON o.product_id = p.id
SET p.quantity = p.quantity + o.quantity
WHERE o.id = @order_id",
                DbHelper.Param("@order_id", orderId.Value));

            ExecuteInTransaction(connection, transaction, @"
UPDATE users u
INNER JOIN orders o ON o.user_id = u.id
SET u.money = u.money + o.total_price
WHERE o.id = @order_id",
                DbHelper.Param("@order_id", orderId.Value));

            ExecuteInTransaction(connection, transaction,
                "DELETE FROM orders WHERE id = @order_id",
                DbHelper.Param("@order_id", orderId.Value));

            transaction.Commit();
            LoadOrders();
            MessageBox.Show("Заказ удалён.");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            MessageBox.Show($"Не удалось удалить заказ: {ex.Message}");
        }
    }

    private void AddPickupPoint_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PickupNameTextBox.Text) ||
            string.IsNullOrWhiteSpace(PickupAddressTextBox.Text))
        {
            MessageBox.Show("Заполните название и адрес пункта выдачи.");
            return;
        }

        _db.ExecuteNonQuery(@"
INSERT INTO pickup_points (name, address, phone, working_hours)
VALUES (@name, @address, @phone, @working_hours)",
            DbHelper.Param("@name", PickupNameTextBox.Text.Trim()),
            DbHelper.Param("@address", PickupAddressTextBox.Text.Trim()),
            DbHelper.Param("@phone", PickupPhoneTextBox.Text.Trim()),
            DbHelper.Param("@working_hours", PickupHoursTextBox.Text.Trim()));

        ClearPickupPointForm();
        LoadPickupPoints();
    }

    private void DeletePickupPoint_Click(object sender, RoutedEventArgs e)
    {
        if (PickupPointsGrid.SelectedItem is not DataRowView row)
        {
            MessageBox.Show("Выберите пункт выдачи.");
            return;
        }

        var pickupPointId = Convert.ToInt32(row["id"]);
        _db.ExecuteNonQuery(
            "DELETE FROM pickup_points WHERE id = @pickup_point_id",
            DbHelper.Param("@pickup_point_id", pickupPointId));
        LoadPickupPoints();
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadOrders();
        LoadPickupPoints();
    }

    private int? GetSelectedOrderId()
    {
        return OrdersGrid.SelectedItem is DataRowView row
            ? Convert.ToInt32(row["order_id"])
            : null;
    }

    private void ClearPickupPointForm()
    {
        PickupNameTextBox.Clear();
        PickupAddressTextBox.Clear();
        PickupPhoneTextBox.Clear();
        PickupHoursTextBox.Clear();
    }

    private static double? ParseNullableDouble(string text)
    {
        return double.TryParse(text, out var value) ? value : null;
    }

    private static void ExecuteInTransaction(MySqlConnection connection, MySqlTransaction transaction, string sql, params MySqlParameter[] parameters)
    {
        using var command = new MySqlCommand(sql, connection, transaction);
        command.Parameters.AddRange(parameters);
        command.ExecuteNonQuery();
    }
}
