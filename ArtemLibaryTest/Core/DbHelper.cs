using MySql.Data.MySqlClient;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;


namespace ArtemLibaryTest.Core
{
    public sealed class DbHelper
    {
        private readonly string _connectionString;

        internal string ConnectionString => _connectionString;

        public DbHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int ExecuteNonQuery(string sql, params MySqlParameter[] parameters)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var command = CreateCommand(connection, sql, parameters);
            return command.ExecuteNonQuery();
        }

        public object? ExecuteScalar(string sql, params MySqlParameter[] parameters)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var command = CreateCommand(connection, sql, parameters);
            return command.ExecuteScalar();
        }

        public DataTable GetTable(string sql, params MySqlParameter[] parameters)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var command = CreateCommand(connection, sql, parameters);
            using var adapter = new MySqlDataAdapter(command);
            var table = new DataTable();
            adapter.Fill(table);

            return table;
        }

        public DataTable GetTableWithImagePath(string sql, params MySqlParameter[] parameters)
        {
            return GetTableWithImagePath(sql, "photo", "Img", "img", parameters);
        }

        public DataTable GetTableWithImagePath(
            string sql,
            string photoColumn,
            string imagePathColumn,
            string imageFolder,
            params MySqlParameter[] parameters)
        {
            var table = GetTable(sql, parameters);
            AddImagePathColumn(table, photoColumn, imagePathColumn, imageFolder);

            return table;
        }

        public DataTable GetTableWithBlobImage(string sql, params MySqlParameter[] parameters)
        {
            return GetTableWithBlobImage(sql, "img", "ImgSource", parameters);
        }

        public DataTable GetTableWithBlobImage(
            string sql,
            string blobColumn,
            string imageSourceColumn,
            params MySqlParameter[] parameters)
        {
            var table = GetTable(sql, parameters);
            AddBlobImageColumn(table, blobColumn, imageSourceColumn);

            return table;
        }

        public DataTable LoadCardTable(string sql, params MySqlParameter[] parameters)
        {
            return GetTable(sql, parameters);
        }

        public DataTable LoadCardTableWithImagePath(string sql, params MySqlParameter[] parameters)
        {
            return GetTableWithImagePath(sql, parameters);
        }


        public static MySqlParameter Param(string name, object? value)
        {
            return new MySqlParameter(name, value ?? DBNull.Value);
        }

        public static void AddWhereEqualsFromComboBox(StringBuilder sql, List<MySqlParameter> parameters, string column, string parameterName, object? selectedValue)
        {
            selectedValue = NormalizeComboBoxSelectedValue(selectedValue);

            if (selectedValue == null)
            {
                return;
            }

            sql.Append($" AND {column} = {parameterName}");
            parameters.Add(Param(parameterName, selectedValue));
        }

        public static void AddWhereEqualsFromComboBox(StringBuilder sql, List<MySqlParameter> parameters, string column, string parameterName, ComboBox comboBox)
        {
            var selectedValue = NormalizeComboBoxSelectedValue(comboBox.SelectedValue);

            if (selectedValue == null && comboBox.SelectedItem is DataRowView rowView)
            {
                selectedValue = GetComboBoxValueFromRow(comboBox, rowView);
            }

            AddWhereEqualsFromComboBox(sql, parameters, column, parameterName, selectedValue);
        }

        public void LoadComboBox(
            ComboBox comboBox,
            string sql,
            string displayColumn = "name",
            string valueColumn = "id",
            string? firstItemText = null,
            params MySqlParameter[] parameters)
        {
            var table = GetTable(sql, parameters);

            if (table.Columns.Count == 0)
            {
                comboBox.ItemsSource = null;
                comboBox.SelectedIndex = -1;
                return;
            }

            var actualDisplayColumn = FindColumnName(table, displayColumn) ?? table.Columns[0].ColumnName;
            var actualValueColumn = FindColumnName(table, valueColumn) ?? table.Columns[0].ColumnName;

            if (!string.IsNullOrWhiteSpace(firstItemText))
            {
                var firstRow = table.NewRow();
                firstRow[actualValueColumn] = DBNull.Value;
                firstRow[actualDisplayColumn] = firstItemText;
                table.Rows.InsertAt(firstRow, 0);
            }

            comboBox.ItemsSource = table.DefaultView;
            comboBox.DisplayMemberPath = actualDisplayColumn;
            comboBox.SelectedValuePath = actualValueColumn;
            comboBox.SelectedIndex = table.Rows.Count > 0 ? 0 : -1;
        }

        public void LoadCategoriesToComboBox(ComboBox comboBox)
        {
            LoadComboBox(
                comboBox,
                "SELECT id, name FROM categories ORDER BY name",
                "name",
                "id",
                "Все категории");
        }

        public static void FillComboBox(ComboBox comboBox, params string[] items)
        {
            comboBox.Items.Clear();

            foreach (string item in items)
            {
                comboBox.Items.Add(item);
            }

            if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }

            comboBox.SelectedIndex = -1;
        }

        private static object? NormalizeComboBoxSelectedValue(object? selectedValue)
        {
            if (selectedValue == null ||
                selectedValue == DBNull.Value ||
                selectedValue == DependencyProperty.UnsetValue ||
                (selectedValue is string text && string.IsNullOrWhiteSpace(text)))
            {
                return null;
            }

            return selectedValue;
        }

        private static object? GetComboBoxValueFromRow(ComboBox comboBox, DataRowView rowView)
        {
            var valueColumn = string.IsNullOrWhiteSpace(comboBox.SelectedValuePath)
                ? null
                : comboBox.SelectedValuePath;

            if (valueColumn != null && rowView.Row.Table.Columns.Contains(valueColumn))
            {
                return NormalizeComboBoxSelectedValue(rowView[valueColumn]);
            }

            return rowView.Row.ItemArray
                .Select(NormalizeComboBoxSelectedValue)
                .FirstOrDefault(value => value != null);
        }

        private static string? FindColumnName(DataTable table, string columnName)
        {
            if (table.Columns.Contains(columnName))
            {
                return columnName;
            }

            return table.Columns
                .Cast<DataColumn>()
                .FirstOrDefault(column => column.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                ?.ColumnName;
        }

        public DataTable GetProductCharacteristics(int productId)
        {
            const string sql = @"
SELECT 1 AS id, id AS product_id, 'Материал' AS name, material AS value, 1 AS display_order
FROM products
WHERE id = @productId AND material <> ''
UNION ALL
SELECT 2 AS id, id AS product_id, 'Цвет' AS name, color AS value, 2 AS display_order
FROM products
WHERE id = @productId AND color <> ''
UNION ALL
SELECT 3 AS id, id AS product_id, 'Размеры' AS name, dimensions AS value, 3 AS display_order
FROM products
WHERE id = @productId AND dimensions <> ''
UNION ALL
SELECT 4 AS id, id AS product_id, 'Описание' AS name, description AS value, 4 AS display_order
FROM products
WHERE id = @productId AND description <> ''
ORDER BY display_order, id;";

            return GetTable(sql, Param("@productId", productId));
        }

        public Border AddCharacteristics(
            StackPanel hostPanel,
            int productId,
            string header = "Характеристики товара")
        {
            var table = GetProductCharacteristics(productId);

            var contentPanel = new StackPanel { Margin = new Thickness(0) };
            if (table.Rows.Count == 0)
            {
                contentPanel.Children.Add(new TextBlock
                {
                    Text = "Характеристики не заполнены.",
                    Margin = new Thickness(0, 4, 0, 0)
                });
            }
            else
            {
                foreach (DataRow row in table.Rows)
                {
                    var name = Convert.ToString(row["name"]) ?? string.Empty;
                    var value = Convert.ToString(row["value"]) ?? string.Empty;
                    contentPanel.Children.Add(new TextBlock
                    {
                        Text = $"• {name}: {value}",
                        Margin = new Thickness(0, 2, 0, 2),
                        TextWrapping = TextWrapping.Wrap
                    });
                }
            }

            var section = new StackPanel();
            section.Children.Add(new TextBlock
            {
                Text = header,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            });
            section.Children.Add(contentPanel);

            var border = CreateCardBorder(section);
            hostPanel.Children.Add(border);
            return border;
        }

        public Border AddCharacteristics(
            StackPanel hostPanel,
            DataRow row,
            string header = "Характеристики товара",
            params string[] characteristicColumns)
        {
            var contentPanel = new StackPanel { Margin = new Thickness(0) };
            var columns = GetCharacteristicColumns(row.Table, characteristicColumns);

            if (columns.Length == 0)
            {
                contentPanel.Children.Add(new TextBlock
                {
                    Text = "Характеристики не заполнены.",
                    Margin = new Thickness(0, 4, 0, 0)
                });
            }
            else
            {
                foreach (var column in columns)
                {
                    var value = row[column] == DBNull.Value
                        ? string.Empty
                        : Convert.ToString(row[column]) ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    contentPanel.Children.Add(new TextBlock
                    {
                        Text = $"• {column}: {value}",
                        Margin = new Thickness(0, 2, 0, 2),
                        TextWrapping = TextWrapping.Wrap
                    });
                }
            }

            var section = new StackPanel();
            section.Children.Add(new TextBlock
            {
                Text = header,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            });
            section.Children.Add(contentPanel);

            var border = CreateCardBorder(section);
            hostPanel.Children.Add(border);
            return border;
        }

        public void AddCardsFromTable(
            StackPanel hostPanel,
            DataTable table,
            string titleColumn = "name",
            params string[] characteristicColumns)
        {
            hostPanel.Children.Clear();

            foreach (DataRow row in table.Rows)
            {
                var title = table.Columns.Contains(titleColumn)
                    ? Convert.ToString(row[titleColumn]) ?? string.Empty
                    : "Карточка";

                AddCharacteristics(hostPanel, row, title, characteristicColumns);
            }
        }

        public Border? ToggleCharacteristicsForCard(Button sourceButton, string headerPrefix = "Характеристики: ")
        {
            if (sourceButton.DataContext is not DataRowView row)
            {
                return null;
            }

            var productId = Convert.ToInt32(row["id"]);
            var productName = Convert.ToString(row["name"]) ?? $"ID {productId}";
            var cardStack = FindParent<StackPanel>(sourceButton);

            if (cardStack == null)
            {
                return null;
            }

            var tag = $"chars_{productId}";
            var existing = cardStack.Children
                .OfType<Border>()
                .FirstOrDefault(x => Equals(x.Tag, tag));

            if (existing != null)
            {
                cardStack.Children.Remove(existing);
                return null;
            }

            var border = AddCharacteristics(cardStack, productId, $"{headerPrefix}{productName}");
            border.Tag = tag;
            return border;
        }

        private static Border CreateCardBorder(UIElement child)
        {
            return new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 8, 0, 0),
                Child = child
            };
        }

        private static string[] GetCharacteristicColumns(DataTable table, string[] characteristicColumns)
        {
            if (characteristicColumns.Length > 0)
            {
                return characteristicColumns
                    .Where(table.Columns.Contains)
                    .ToArray();
            }

            var skipColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "id",
                "name",
                "photo",
                "img",
                "Img",
                "ImgSource"
            };

            return table.Columns
                .Cast<DataColumn>()
                .Select(column => column.ColumnName)
                .Where(columnName => !skipColumns.Contains(columnName))
                .ToArray();
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

        public static void AddWhereEquals(StringBuilder sql, List<MySqlParameter> parameters, string column, string parameterName, object? value)
        {
            if (value == null)
            {
                return;
            }

            sql.Append($" AND {column} = {parameterName}");
            parameters.Add(Param(parameterName, value));
        }

        public static void AddWhereMin(StringBuilder sql, List<MySqlParameter> parameters, string column, string parameterName, double? value)
        {
            if (!value.HasValue)
            {
                return;
            }

            sql.Append($" AND {column} >= {parameterName}");
            parameters.Add(Param(parameterName, value.Value));
        }

        public static void AddWhereMax(StringBuilder sql, List<MySqlParameter> parameters, string column, string parameterName, double? value)
        {
            if (!value.HasValue)
            {
                return;
            }

            sql.Append($" AND {column} <= {parameterName}");
            parameters.Add(Param(parameterName, value.Value));
        }

        public static void AddWhereLike(StringBuilder sql, List<MySqlParameter> parameters, string column, string parameterName, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            sql.Append($" AND {column} LIKE {parameterName}");
            parameters.Add(Param(parameterName, $"%{value.Trim()}%"));
        }

        public static void AddWhereLikeAnyWord(StringBuilder sql, List<MySqlParameter> parameters, string column, string parameterName, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var words = value
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (words.Length == 0)
            {
                return;
            }

            sql.Append(" AND (");

            for (var i = 0; i < words.Length; i++)
            {
                if (i > 0)
                {
                    sql.Append(" OR ");
                }

                var wordParameterName = $"{parameterName}{i}";
                sql.Append($"{column} LIKE {wordParameterName}");
                parameters.Add(Param(wordParameterName, $"%{words[i]}%"));
            }

            sql.Append(')');
        }

        public static void AddImagePathColumn(
            DataTable table,
            string photoColumn = "photo",
            string imagePathColumn = "Img",
            string imageFolder = "img")
        {
            if (!table.Columns.Contains(photoColumn) || table.Columns.Contains(imagePathColumn))
            {
                return;
            }

            table.Columns.Add(imagePathColumn, typeof(string));
            var imagesRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imageFolder);

            foreach (DataRow row in table.Rows)
            {
                var fileName = row[photoColumn]?.ToString();
                row[imagePathColumn] = Path.Combine(imagesRoot, string.IsNullOrWhiteSpace(fileName) ? "default.png" : fileName);
            }
        }

        public static void AddBlobImageColumn(
            DataTable table,
            string blobColumn = "img",
            string imageSourceColumn = "ImgSource")
        {
            if (!table.Columns.Contains(blobColumn) || table.Columns.Contains(imageSourceColumn))
            {
                return;
            }

            table.Columns.Add(imageSourceColumn, typeof(ImageSource));

            foreach (DataRow row in table.Rows)
            {
                row[imageSourceColumn] = (object?)TryCreateImageSource(row[blobColumn]) ?? DBNull.Value;
            }
        }

        private static ImageSource? TryCreateImageSource(object? blobValue)
        {
            try
            {
                var imageBytes = GetBlobBytes(blobValue);
                if (imageBytes.Length == 0)
                {
                    return null;
                }

                using var stream = new MemoryStream(imageBytes);
                var bitmap = BitmapFrame.Create(
                    stream,
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad);
                bitmap.Freeze();

                return bitmap;
            }
            catch (NotSupportedException)
            {
                return null;
            }
            catch (FileFormatException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private static byte[] GetBlobBytes(object? blobValue)
        {
            return blobValue switch
            {
                byte[] bytes => bytes,
                Stream stream => ReadAllBytes(stream),
                string base64 when !string.IsNullOrWhiteSpace(base64) => Convert.FromBase64String(base64),
                _ => []
            };
        }

        private static byte[] ReadAllBytes(Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            return memoryStream.ToArray();
        }

        private static MySqlCommand CreateCommand(MySqlConnection connection, string sql, params MySqlParameter[] parameters)
        {
            var command = new MySqlCommand(sql, connection);

            if (parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return command;
        }
    }
}
