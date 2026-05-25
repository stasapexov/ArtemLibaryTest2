using System.Data;
using System.Windows;
using System.Windows.Controls;
using ArtemLibaryTest.Core;

namespace SampleWpf
{
    public partial class MainWindow : Window
    {
        private readonly DbHelper _db;

        public MainWindow()
        {
            InitializeComponent();
            _db = new DbHelper(DbConfig.ConnectionString);
            LoadData();
        }

        private void LoadData()
        {
            var sql = @"SELECT p.id, p.name, c.name AS category_name, p.quantity, p.price
                        FROM products p
                        LEFT JOIN categories c ON c.id = p.category_id
                        ORDER BY p.id;";
            var dt = _db.GetTable(sql);
            ItemsData.ItemsSource = dt.DefaultView;
        }

        private void Characteristick_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }

            var result = _db.ToggleCharacteristicsForCard(button);
            if (result == null && button.DataContext is not DataRowView)
            {
                MessageBox.Show("Не удалось показать характеристики: отсутствуют данные товара.");
            }
        }
    }
}
