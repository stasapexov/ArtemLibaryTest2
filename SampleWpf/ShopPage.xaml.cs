using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using ArtemLibaryTest.Core;

namespace SampleWpf;

public partial class ShopPage : Page
{
    private readonly DbHelper _db = new(DbConfig.ConnectionString);

    public ShopPage()
    {
        InitializeComponent();
        ConfigureMode();
        LoadProducts();
    }

    private bool CanOrder => string.Equals(Session.CurrentUser?.Status, "user", StringComparison.OrdinalIgnoreCase);

    private void ConfigureMode()
    {
        OrderButton.IsEnabled = CanOrder;
        QuantityBox.IsEnabled = CanOrder;
        ModeText.Text = CanOrder
            ? "Выберите товар, укажите количество и оформите заказ."
            : "Гость может только смотреть товары. Для заказа войдите как пользователь.";
    }

    private void LoadProducts()
    {
        const string sql = @"
SELECT p.id, p.name AS product_name, c.name AS category, p.quantity, p.price,
       p.material, p.color, p.dimensions, p.description
FROM products p
LEFT JOIN categories c ON c.id = p.category_id
ORDER BY p.name;";
        ProductsGrid.ItemsSource = _db.GetTable(sql).DefaultView;
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadProducts();
    }

    private void OrderButton_Click(object sender, RoutedEventArgs e)
    {
        if (!CanOrder || Session.CurrentUser == null)
        {
            MessageBox.Show("Заказывать товары может только пользователь.");
            return;
        }

        if (ProductsGrid.SelectedItem is not DataRowView row)
        {
            MessageBox.Show("Выберите товар.");
            return;
        }

        if (!int.TryParse(QuantityBox.Text.Trim(), out var quantity) || quantity <= 0)
        {
            MessageBox.Show("Введите положительное целое количество.");
            return;
        }

        var productId = Convert.ToInt32(row["id"]);
        const string insertSql = @"
INSERT INTO orders (date, user_id, product_id, product_name, product_material, product_color, product_dimensions, quantity, unit_price, total_price, readiness)
SELECT CURRENT_DATE(), @userId, id, name, material, color, dimensions, @quantity, price, price * @quantity, 'Новый'
FROM products
WHERE id = @productId AND quantity >= @quantity;";

        var inserted = _db.ExecuteNonQuery(
            insertSql,
            DbHelper.Param("@userId", Session.CurrentUser.Id),
            DbHelper.Param("@productId", productId),
            DbHelper.Param("@quantity", quantity));

        if (inserted == 0)
        {
            MessageBox.Show("Недостаточно товара на складе.");
            return;
        }

        _db.ExecuteNonQuery(
            "UPDATE products SET quantity = quantity - @quantity WHERE id = @productId;",
            DbHelper.Param("@quantity", quantity),
            DbHelper.Param("@productId", productId));

        MessageBox.Show("Заказ оформлен.");
        LoadProducts();
    }
}
