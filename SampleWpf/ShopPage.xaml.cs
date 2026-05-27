using ArtemLibaryTest.Core;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SampleWpf;

public partial class ShopPage : Page
{
    private readonly DbHelper _db = new(DbConfig.ConnectionString);
    private readonly string _role;
    private readonly bool _isAuthorized;

    public ShopPage(string role, bool isAuthorized)
    {
        InitializeComponent();
        _role = role.ToLowerInvariant();
        _isAuthorized = isAuthorized;
        Filter_Border.Visibility = (_role == "admin" || _role == "manager") ? Visibility.Visible : Visibility.Collapsed;
        LoadCombo();
        LoadData();
        LoadMoney();
    }

    private void LoadMoney() => LableMoney.Text = _isAuthorized ? "На счету: demo" : "Гость";

    private void LoadCombo()
    {
        var categories = _db.GetTable("SELECT id, name FROM categories");
        var allRow = categories.NewRow(); allRow["id"] = DBNull.Value; allRow["name"] = "Все категории"; categories.Rows.InsertAt(allRow, 0);
        ComboData.ItemsSource = categories.DefaultView; ComboData.SelectedIndex = 0;
    }

    private void LoadData(double? min = null, double? max = null, string name = "")
    {
        var sql = new StringBuilder(@"SELECT p.id, p.name, c.name AS category_name, p.quantity, p.price, p.photo FROM products p LEFT JOIN categories c ON c.id = p.category_id WHERE 1=1");
        var parameter = new List<MySqlParameter>();
        if (_role == "admin" || _role == "manager")
        {
            DbHelper.AddWhereMin(sql, parameter, "p.price", "@min", min);
            DbHelper.AddWhereMax(sql, parameter, "p.price", "@max", max);
            DbHelper.AddWhereLikeAnyWord(sql, parameter, "p.name", "@name", name);
            DbHelper.AddWhereEqualsFromComboBox(sql, parameter, "p.category_id", "@category_id", ComboData.SelectedValue);
        }

        var dt = _db.GetTableWithImagePath(sql.ToString(), parameter.ToArray());
        if (!dt.Columns.Contains("IsUserVisible")) dt.Columns.Add("IsUserVisible", typeof(Visibility));
        if (!dt.Columns.Contains("IsAdminVisible")) dt.Columns.Add("IsAdminVisible", typeof(Visibility));
        foreach (DataRow row in dt.Rows)
        {
            row["IsUserVisible"] = _isAuthorized && _role == "user" ? Visibility.Visible : Visibility.Collapsed;
            row["IsAdminVisible"] = _isAuthorized && _role == "admin" ? Visibility.Visible : Visibility.Collapsed;
        }
        ItemsData.ItemsSource = dt.DefaultView;
    }

    private void Filter_Click(object sender, RoutedEventArgs e)
    {
        double? min = double.TryParse(TbMin.Text, out var minValue) ? minValue : null;
        double? max = double.TryParse(TbMax.Text, out var maxValue) ? maxValue : null;
        LoadData(min, max, TbName.Text);
    }

    private void Order_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Заказ оформлен (демо).");
    private void Edit_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Редактирование товара (демо админ).");

    private void Count_Change_Click(object sender, TextChangedEventArgs e)
    {
        var tb = (TextBox)sender;
        var row = (DataRowView)tb.DataContext;
        var stack = (StackPanel)((DockPanel)tb.Parent).Parent;
        var textBlock = (TextBlock)stack.FindName("TotalPriceLable");
        var price = Convert.ToDouble(row["price"]);
        var quantity = int.TryParse(tb.Text, out var count) ? count : 1;
        textBlock.Text = $"Итоговая цена: {price * quantity}";
    }

    private void Characteristick_Click(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        _db.ToggleCharacteristicsForCard(button);
    }
}
