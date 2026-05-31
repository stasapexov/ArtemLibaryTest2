using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ArtemLibaryTest.Core;
using MySql.Data.MySqlClient;

namespace SampleWpf.Pages;

public partial class ShopPage : Page
{
    private readonly DbHelper _db = new(DbConfig.ConnectionString);

    public ShopPage()
    {
        InitializeComponent();
        LoadComboBoxes();
        LoadData();
        UpdateUserInfo();
    }

    private void LoadComboBoxes()
    {
        _db.LoadCategoriesToComboBox(CategoryComboBox);
        _db.LoadComboBox(
            PickupPointComboBox,
            @"SELECT id, CONCAT(name, ' — ', address) AS address
              FROM pickup_points
              ORDER BY address, name",
            "address",
            "id",
            "Выберите пункт выдачи");
    }

    private void LoadData(double? min = null, double? max = null, string name = "")
    {
        var sql = new StringBuilder(@"
SELECT p.id, p.name, c.name AS category_name, p.quantity, p.price, p.photo,
       p.dimensions
FROM products p
LEFT JOIN categories c ON c.id = p.category_id
WHERE 1=1");
        var parameters = new List<MySqlParameter>();

        DbHelper.AddWhereMin(sql, parameters, "p.price", "@min", min);
        DbHelper.AddWhereMax(sql, parameters, "p.price", "@max", max);
        DbHelper.AddWhereLikeAnyWord(sql, parameters, "p.name", "@name", name);
        DbHelper.AddWhereEqualsFromComboBox(sql, parameters, "p.category_id", "@category", CategoryComboBox);

        sql.Append(" ORDER BY p.name");
        var table = _db.GetTableWithImagePath(sql.ToString(), parameters.ToArray());
        ItemsData.ItemsSource = table.DefaultView;
    }

    private void UpdateUserInfo()
    {
        var user = Session.CurrentUser;
        MoneyTextBlock.Text = user == null
            ? "Пользователь не выбран"
            : $"Пользователь: {user.Login}, баланс: {user.Money:0.00}";
    }

    private void Order_Click(object sender, RoutedEventArgs e)
    {
        if (Session.CurrentUser == null || Session.IsGuest)
        {
            MessageBox.Show("Войдите, чтобы заказать товар.");
            return;
        }

        if (PickupPointComboBox.SelectedValue == null ||
            PickupPointComboBox.SelectedValue == DBNull.Value ||
            PickupPointComboBox.SelectedValue == DependencyProperty.UnsetValue)
        {
            MessageBox.Show("Выберите пункт выдачи.");
            return;
        }

        if (sender is not Button button || button.DataContext is not DataRowView row)
        {
            return;
        }

        var countBox = FindChild<TextBox>(button, "CountTextBox");
        var count = int.TryParse(countBox?.Text, out var parsedCount) && parsedCount > 0 ? parsedCount : 1;
        var quantity = Convert.ToInt32(row["quantity"]);

        if (quantity < count)
        {
            MessageBox.Show("На складе нет такого количества товара.");
            return;
        }

        var price = Convert.ToDecimal(row["price"]);
        var totalPrice = price * count;

        if ((decimal)Session.CurrentUser.Money < totalPrice)
        {
            MessageBox.Show("Недостаточно денег на балансе.");
            return;
        }

        var pickupPointId = Convert.ToInt32(PickupPointComboBox.SelectedValue);
        var pickupAddress = GetSelectedPickupAddress();
        var pickupCode = GeneratePickupCode();
        var userId = Session.CurrentUser.Id;
        var productId = Convert.ToInt32(row["id"]);

        using var connection = new MySqlConnection(DbConfig.ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            using var insertOrder = new MySqlCommand(@"
INSERT INTO orders
(date, user_id, product_id, product_name, product_photo, product_dimensions,
 quantity, unit_price, total_price, readiness, pickup_point_id, pickup_address, pickup_code)
VALUES
(CURDATE(), @user_id, @product_id, @product_name, @product_photo, @product_dimensions,
 @quantity, @unit_price, @total_price, 'New', @pickup_point_id, @pickup_address, @pickup_code);", connection, transaction);
            insertOrder.Parameters.AddRange(new MySqlParameter[]
            {
                DbHelper.Param("@user_id", userId),
                DbHelper.Param("@product_id", productId),
                DbHelper.Param("@product_name", Convert.ToString(row["name"]) ?? string.Empty),
                DbHelper.Param("@product_photo", Convert.ToString(row["photo"]) ?? string.Empty),
                DbHelper.Param("@product_dimensions", Convert.ToString(row["dimensions"]) ?? string.Empty),
                DbHelper.Param("@quantity", count),
                DbHelper.Param("@unit_price", price),
                DbHelper.Param("@total_price", totalPrice),
                DbHelper.Param("@pickup_point_id", pickupPointId),
                DbHelper.Param("@pickup_address", pickupAddress),
                DbHelper.Param("@pickup_code", pickupCode)
            });
            insertOrder.ExecuteNonQuery();

            ExecuteInTransaction(connection, transaction,
                "UPDATE products SET quantity = quantity - @quantity WHERE id = @product_id",
                DbHelper.Param("@quantity", count),
                DbHelper.Param("@product_id", productId));

            ExecuteInTransaction(connection, transaction,
                "UPDATE users SET money = money - @total_price WHERE id = @user_id",
                DbHelper.Param("@total_price", totalPrice),
                DbHelper.Param("@user_id", userId));

            transaction.Commit();
            Session.CurrentUser.Money -= Convert.ToDouble(totalPrice);
            MessageBox.Show($"Заказ создан. Код получения: {pickupCode}\nПункт выдачи: {pickupAddress}");
            LoadData();
            UpdateUserInfo();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            MessageBox.Show($"Не удалось создать заказ: {ex.Message}");
        }
    }

    private void CountTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox || textBox.DataContext is not DataRowView row)
        {
            return;
        }

        var totalText = FindChild<TextBlock>(textBox, "TotalPriceTextBlock");
        var count = int.TryParse(textBox.Text, out var parsedCount) && parsedCount > 0 ? parsedCount : 1;
        var price = Convert.ToDecimal(row["price"]);

        if (totalText != null)
        {
            totalText.Text = $"Итоговая цена: {price * count:0.00}";
        }
    }

    private void Characteristic_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            _db.ToggleCharacteristicsForCard(button);
        }
    }

    private void Filter_Click(object sender, RoutedEventArgs e)
    {
        var min = ParseNullableDouble(TbMin.Text);
        var max = ParseNullableDouble(TbMax.Text);
        LoadData(min, max, TbName.Text);
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadComboBoxes();
        LoadData();
        UpdateUserInfo();
    }

    private string GetSelectedPickupAddress()
    {
        return PickupPointComboBox.SelectedItem is DataRowView row
            ? Convert.ToString(row["address"]) ?? string.Empty
            : string.Empty;
    }

    private static string GeneratePickupCode()
    {
        return Random.Shared.Next(100000, 999999).ToString();
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

    private static T? FindChild<T>(DependencyObject start, string name) where T : FrameworkElement
    {
        var parent = VisualTreeHelper.GetParent(start);

        while (parent != null && parent is not ContentPresenter)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }

        if (parent == null)
        {
            return null;
        }

        return FindVisualChild<T>(parent, name);
    }

    private static T? FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typedChild && typedChild.Name == name)
            {
                return typedChild;
            }

            var result = FindVisualChild<T>(child, name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
