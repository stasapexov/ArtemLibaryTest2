using ArtemLibaryTest.Core;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace ArtemLibaryTest.QuickStart
{
    public partial class ProductsOrderPage : Page
    {
        private readonly AuthUiContext _context;

        public ProductsOrderPage(AuthUiContext context)
        {
            InitializeComponent();
            _context = context;
            LoadCategories();
            LoadProducts();
        }

        private MySqlAuthService? MySql => _context.AuthService as MySqlAuthService;

        private void LoadCategories()
        {
            if (MySql == null) return;
            var table = MySql.GetCategories();
            var allRow = table.NewRow();
            allRow["id"] = DBNull.Value;
            allRow["name"] = "Все категории";
            table.Rows.InsertAt(allRow, 0);
            CategoryComboBox.ItemsSource = table.DefaultView;
            CategoryComboBox.SelectedIndex = 0;
        }

        private void LoadProducts()
        {
            if (MySql == null) return;
            decimal? min = decimal.TryParse(MinPriceTextBox.Text, out var minVal) ? minVal : null;
            decimal? max = decimal.TryParse(MaxPriceTextBox.Text, out var maxVal) ? maxVal : null;
            int? categoryId = CategoryComboBox.SelectedValue == null || CategoryComboBox.SelectedValue == DBNull.Value
                ? null
                : Convert.ToInt32(CategoryComboBox.SelectedValue);

            ProductsGrid.ItemsSource = MySql.GetStoreProducts(NameFilterTextBox.Text, min, max, categoryId).DefaultView;
        }

        private void Filter_Click(object sender, RoutedEventArgs e) => LoadProducts();

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            NameFilterTextBox.Text = string.Empty;
            MinPriceTextBox.Text = string.Empty;
            MaxPriceTextBox.Text = string.Empty;
            CategoryComboBox.SelectedIndex = 0;
            LoadProducts();
        }

        private void Order_Click(object sender, RoutedEventArgs e)
        {
            if (MySql == null || Session.CurrentUser == null) return;
            if (ProductsGrid.SelectedItem is not DataRowView row)
            {
                MessageBox.Show("Выберите товар.");
                return;
            }

            if (!decimal.TryParse(QuantityTextBox.Text, out var qty) || qty <= 0)
            {
                MessageBox.Show("Введите корректное количество.");
                return;
            }

            var productId = Convert.ToInt32(row["id"]);
            var success = MySql.CreateOrder(Session.CurrentUser.Id, productId, qty);
            MessageBox.Show(success ? "Заказ создан." : "Не удалось создать заказ.");
            LoadProducts();
        }
    }
}
