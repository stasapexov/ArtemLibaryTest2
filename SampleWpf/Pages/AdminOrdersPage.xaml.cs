using System.Data;
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
        var table = _db.GetTable(@"
SELECT
    o.id AS order_id,
    o.date AS order_date,
    u.login AS user_login,
    o.product_name,
    o.quantity,
    o.total_price,
    o.readiness,
    o.pickup_address,
    o.pickup_code
FROM orders o
INNER JOIN users u ON u.id = o.user_id
ORDER BY o.id DESC");

        OrdersGrid.ItemsSource = table.DefaultView;
        OrdersInfoTextBlock.Text = $"Заказов: {table.Rows.Count}";
    }

    private void LoadPickupPoints()
    {
        var table = _db.GetTable(@"
SELECT id, name, address, phone, working_hours
FROM pickup_points
ORDER BY address, name");
        PickupPointsGrid.ItemsSource = table.DefaultView;
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
INNER JOIN order_items oi ON oi.product_id = p.id
SET p.quantity = p.quantity + oi.quantity
WHERE oi.order_id = @order_id",
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

    private static void ExecuteInTransaction(MySqlConnection connection, MySqlTransaction transaction, string sql, params MySqlParameter[] parameters)
    {
        using var command = new MySqlCommand(sql, connection, transaction);
        command.Parameters.AddRange(parameters);
        command.ExecuteNonQuery();
    }
}
