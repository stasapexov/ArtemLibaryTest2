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
SELECT order_id, order_date, user_login, product_name, quantity, total_price, readiness, pickup_address, pickup_code
FROM v_order_details
ORDER BY order_id DESC");

        OrdersGrid.ItemsSource = table.DefaultView;
        OrdersInfoTextBlock.Text = $"Заказов: {table.Rows.Count}";
    }

    private void LoadPickupPoints()
    {
        var table = _db.GetTable(@"
SELECT id, name, city, street, house, phone, working_hours
FROM pickup_points
ORDER BY city, street, house, name");
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
            string.IsNullOrWhiteSpace(PickupCityTextBox.Text) ||
            string.IsNullOrWhiteSpace(PickupStreetTextBox.Text) ||
            string.IsNullOrWhiteSpace(PickupHouseTextBox.Text))
        {
            MessageBox.Show("Заполните название, город, улицу и дом пункта выдачи.");
            return;
        }

        _db.ExecuteNonQuery(@"
INSERT INTO pickup_points (name, city, street, house, phone, working_hours)
VALUES (@name, @city, @street, @house, @phone, @working_hours)",
            DbHelper.Param("@name", PickupNameTextBox.Text.Trim()),
            DbHelper.Param("@city", PickupCityTextBox.Text.Trim()),
            DbHelper.Param("@street", PickupStreetTextBox.Text.Trim()),
            DbHelper.Param("@house", PickupHouseTextBox.Text.Trim()),
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
        PickupCityTextBox.Clear();
        PickupStreetTextBox.Clear();
        PickupHouseTextBox.Clear();
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
