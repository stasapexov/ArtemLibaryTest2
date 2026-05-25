using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
            var button = (Button)sender;
            var row = (DataRowView)button.DataContext;
            var productId = Convert.ToInt32(row["id"]);
            var productName = Convert.ToString(row["name"]) ?? $"ID {productId}";

            var cardStack = FindParent<StackPanel>(button);
            if (cardStack == null)
            {
                MessageBox.Show("Не найден контейнер карточки товара.");
                return;
            }

            var tag = $"chars_{productId}";
            var existing = cardStack.Children
                .OfType<Border>()
                .FirstOrDefault(x => Equals(x.Tag, tag));

            if (existing != null)
            {
                cardStack.Children.Remove(existing);
                return;
            }

            var border = _db.AddCharacteristics(cardStack, productId, $"Характеристики: {productName}");
            border.Tag = tag;
        }

        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject? parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T typedParent)
                {
                    return typedParent;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }
    }
}
