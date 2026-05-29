using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using ArtemLibaryTest.Core;

namespace SampleWpf;

public partial class ProductsPage : Page
{
    private readonly DbHelper _db = new(DbConfig.ConnectionString);

    public ProductsPage()
    {
        InitializeComponent();
        ConfigureRole();
        LoadProducts();
    }

    private bool IsAdmin => string.Equals(Session.CurrentUser?.Status, "admin", StringComparison.OrdinalIgnoreCase);

    private void ConfigureRole()
    {
        EditorPanel.Visibility = IsAdmin ? Visibility.Visible : Visibility.Collapsed;
        RoleHintText.Text = IsAdmin
            ? "Администратор может редактировать и удалять товары."
            : "Менеджер может только просматривать товары.";
    }

    private void LoadProducts()
    {
        const string sql = @"
SELECT p.id, p.name, p.category_id, c.name AS category, p.quantity, p.price,
       p.photo, p.material, p.color, p.dimensions, p.description
FROM products p
LEFT JOIN categories c ON c.id = p.category_id
ORDER BY p.id;";
        ProductsGrid.ItemsSource = _db.GetTable(sql).DefaultView;
    }

    private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsAdmin || ProductsGrid.SelectedItem is not DataRowView row)
        {
            return;
        }

        NameBox.Text = Convert.ToString(row["name"]);
        CategoryIdBox.Text = Convert.ToString(row["category_id"]);
        QuantityBox.Text = Convert.ToString(row["quantity"]);
        PriceBox.Text = Convert.ToString(row["price"]);
        PhotoBox.Text = Convert.ToString(row["photo"]);
        MaterialBox.Text = Convert.ToString(row["material"]);
        ColorBox.Text = Convert.ToString(row["color"]);
        DimensionsBox.Text = Convert.ToString(row["dimensions"]);
        DescriptionBox.Text = Convert.ToString(row["description"]);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!IsAdmin || ProductsGrid.SelectedItem is not DataRowView row)
        {
            MessageBox.Show("Выберите товар для редактирования.");
            return;
        }

        if (!int.TryParse(CategoryIdBox.Text.Trim(), out var categoryId)
            || !int.TryParse(QuantityBox.Text.Trim(), out var quantity)
            || !decimal.TryParse(PriceBox.Text.Trim(), out var price))
        {
            MessageBox.Show("Проверьте id категории, остаток и цену.");
            return;
        }

        const string sql = @"
UPDATE products
SET name = @name,
    category_id = @categoryId,
    quantity = @quantity,
    price = @price,
    photo = @photo,
    material = @material,
    color = @color,
    dimensions = @dimensions,
    description = @description
WHERE id = @id;";

        _db.ExecuteNonQuery(
            sql,
            DbHelper.Param("@name", NameBox.Text.Trim()),
            DbHelper.Param("@categoryId", categoryId),
            DbHelper.Param("@quantity", quantity),
            DbHelper.Param("@price", price),
            DbHelper.Param("@photo", string.IsNullOrWhiteSpace(PhotoBox.Text) ? "default.png" : PhotoBox.Text.Trim()),
            DbHelper.Param("@material", MaterialBox.Text.Trim()),
            DbHelper.Param("@color", ColorBox.Text.Trim()),
            DbHelper.Param("@dimensions", DimensionsBox.Text.Trim()),
            DbHelper.Param("@description", DescriptionBox.Text.Trim()),
            DbHelper.Param("@id", Convert.ToInt32(row["id"])));

        LoadProducts();
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (!IsAdmin || ProductsGrid.SelectedItem is not DataRowView row)
        {
            MessageBox.Show("Выберите товар для удаления.");
            return;
        }

        var productId = Convert.ToInt32(row["id"]);
        _db.ExecuteNonQuery("DELETE FROM products WHERE id = @id;", DbHelper.Param("@id", productId));
        LoadProducts();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadProducts();
    }
}
