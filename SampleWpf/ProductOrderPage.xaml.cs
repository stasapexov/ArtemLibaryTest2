using ArtemLibaryTest.Core;
using ArtemLibaryTest.Models;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SampleWpf;

public partial class ProductOrderPage : Page
{
    private readonly DbHelper _db = new(DbConfig.ConnectionString);

    public ProductOrderPage()
    {
        InitializeComponent();
        LoadCategories();
        LoadProducts();
    }

    private void LoadCategories()
    {
        var categories = _db.GetTable("SELECT id, name FROM categories ORDER BY name;");
        var row = categories.NewRow(); row["id"] = DBNull.Value; row["name"] = "Все"; categories.Rows.InsertAt(row, 0);
        CbCategory.ItemsSource = categories.DefaultView; CbCategory.SelectedIndex = 0;
    }

    private void LoadProducts()
    {
        var sql = new StringBuilder(@"SELECT p.id, p.name AS material_name, IFNULL(c.name,'Без категории') AS category, p.quantity, p.price, p.photo AS Img FROM products p LEFT JOIN categories c ON c.id=p.category_id WHERE 1=1");
        var parameters = new List<MySqlParameter>();
        DbHelper.AddWhereLike(sql, parameters, "p.name", "@name", TbName.Text);
        DbHelper.AddWhereMin(sql, parameters, "p.price", "@min", double.TryParse(TbMin.Text, out var min) ? min : null);
        DbHelper.AddWhereMax(sql, parameters, "p.price", "@max", double.TryParse(TbMax.Text, out var max) ? max : null);
        if (CbCategory.SelectedValue != null && CbCategory.SelectedValue != DBNull.Value) { sql.Append(" AND p.category_id=@cat"); parameters.Add(DbHelper.Param("@cat", CbCategory.SelectedValue)); }
        ProductsItems.ItemsSource = _db.GetTable(sql.ToString(), parameters.ToArray()).DefaultView;
    }

    private void Filter_Click(object sender, RoutedEventArgs e) => LoadProducts();
    private void Text_Click(object sender, TextChangedEventArgs e) { }

    private void Order_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not DataRowView row) return;
        if (!TryFindSiblingCount(sender as DependencyObject, out var count)) return;
        if (count <= 0) { MessageBox.Show("Введите корректное количество"); return; }

        var productId = Convert.ToInt32(row["id"]);
        var price = Convert.ToDecimal(row["price"]);
        var total = price * count;
        var userId = Session.CurrentUser?.Id ?? 49;

        _db.ExecuteNonQuery("INSERT INTO orders (`date`, user_id, total_price, readiness) VALUES (CURDATE(), @u, @t, 'не готов');", DbHelper.Param("@u", userId), DbHelper.Param("@t", total));
        var orderId = Convert.ToInt32(_db.ExecuteScalar("SELECT LAST_INSERT_ID();"));
        _db.ExecuteNonQuery("INSERT INTO order_items(order_id, product_id, quantity, unit_price) VALUES (@o,@p,@q,@up);",
            DbHelper.Param("@o", orderId), DbHelper.Param("@p", productId), DbHelper.Param("@q", count), DbHelper.Param("@up", price));
        _db.ExecuteNonQuery("UPDATE products SET quantity = quantity - @q WHERE id = @p AND quantity >= @q;", DbHelper.Param("@q", count), DbHelper.Param("@p", productId));
        MessageBox.Show("Заказ создан");
        LoadProducts();
    }

    private static bool TryFindSiblingCount(DependencyObject? source, out decimal count)
    {
        count = 0;
        var border = FindAncestor<Border>(source);
        if (border == null) return false;
        var tb = FindVisualChildByName<TextBox>(border, "TbCount");
        return tb != null && decimal.TryParse(tb.Text, out count);
    }

    private static T? FindAncestor<T>(DependencyObject? d) where T : DependencyObject
    {
        while (d != null && d is not T) d = System.Windows.Media.VisualTreeHelper.GetParent(d);
        return d as T;
    }

    private static T? FindVisualChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T f && f.Name == name) return f;
            var nested = FindVisualChildByName<T>(child, name);
            if (nested != null) return nested;
        }
        return null;
    }
}
