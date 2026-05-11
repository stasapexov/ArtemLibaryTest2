using MySql.Data.MySqlClient;
using System.Data;
using System.IO;
using System.Text;

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

        public static MySqlParameter Param(string name, object? value)
        {
            return new MySqlParameter(name, value ?? DBNull.Value);
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