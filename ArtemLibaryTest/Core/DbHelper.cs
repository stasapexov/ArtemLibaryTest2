using MySql.Data.MySqlClient;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace ArtemLibaryTest.Core
{
    public sealed class DbHelper
    {
        private readonly string _connectionString;

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


        public static MySqlParameter Param(string name, object? value)
        {
            return new MySqlParameter(name, value ?? DBNull.Value);
        }
        public static void AddWhereEqualsFromComboBox(StringBuilder sql, List<MySqlParameter> parameters, string column, string parameterName, object? selectedValue)
        {
            if (selectedValue == null || selectedValue == DBNull.Value)
            {
                return;
            }

            column = NormalizeUnqualifiedIdColumn(column);
            EnsureCanAppendConditions(sql);
            sql.Append($" AND {column} = {parameterName}");
            parameters.Add(Param(parameterName, selectedValue));
        }

        public void LoadCategoriesToComboBox(ComboBox comboBox)
        {
            DataTable categories = GetTable("SELECT id, name FROM categories");
            DataRow allRow = categories.NewRow();
            allRow["id"] = DBNull.Value;
            allRow["name"] = "Все категории";
            categories.Rows.InsertAt(allRow, 0);

            comboBox.ItemsSource = categories.DefaultView;
            comboBox.SelectedIndex = 0;
        }

        public static void AddWhereEquals(StringBuilder sql, List<MySqlParameter> parameters, string column, string parameterName, object? value)
        {
            if (value == null)
            {
                return;
            }

            EnsureCanAppendConditions(sql);
            sql.Append($" AND {column} = {parameterName}");
            parameters.Add(Param(parameterName, value));
        }

        public static void AddWhereMin(StringBuilder sql, List<MySqlParameter> parameters, string column, string parameterName, double? value)
        {
            if (!value.HasValue)
            {
                return;
            }

            EnsureCanAppendConditions(sql);
            sql.Append($" AND {column} >= {parameterName}");
            parameters.Add(Param(parameterName, value.Value));
        }

        public static void AddWhereMax(StringBuilder sql, List<MySqlParameter> parameters, string column, string parameterName, double? value)
        {
            if (!value.HasValue)
            {
                return;
            }

            EnsureCanAppendConditions(sql);
            sql.Append($" AND {column} <= {parameterName}");
            parameters.Add(Param(parameterName, value.Value));
        }

        public static void AddWhereLike(StringBuilder sql, List<MySqlParameter> parameters, string column, string parameterName, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            EnsureCanAppendConditions(sql);
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

            EnsureCanAppendConditions(sql);
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

        private static void EnsureCanAppendConditions(StringBuilder sql)
        {
            while (sql.Length > 0 && char.IsWhiteSpace(sql[^1]))
            {
                sql.Length--;
            }

            if (sql.Length > 0 && sql[^1] == ';')
            {
                sql.Length--;
            }
        }

        private static string NormalizeUnqualifiedIdColumn(string column)
        {
            if (string.Equals(column?.Trim(), "id", StringComparison.OrdinalIgnoreCase))
            {
                return "p.id";
            }

            return column;
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
