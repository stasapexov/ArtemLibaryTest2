using ArtemLibaryTest.Models;
using MySql.Data.MySqlClient;

namespace ArtemLibaryTest.Core
{
    public class MySqlAuthService : IAuthService
    {
        private readonly string _connectionString;

        public MySqlAuthService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Users? Login(string login, string password)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
SELECT id, name, password, login, phone, status
FROM users
WHERE login = @login AND password = @password
LIMIT 1;";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@login", login);
            command.Parameters.AddWithValue("@password", password);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return new Users(
                reader.GetInt32("id"),
                reader.GetString("name"),
                reader.GetString("password"),
                reader.GetString("login"),
                reader.GetString("phone"),
                reader.GetString("status"));
        }

        public bool Register(string login, string password, string name, string phone)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            const string checkSql = "SELECT COUNT(*) FROM users WHERE login = @login;";
            using (var checkCommand = new MySqlCommand(checkSql, connection))
            {
                checkCommand.Parameters.AddWithValue("@login", login);
                var existingUsers = Convert.ToInt32(checkCommand.ExecuteScalar());
                if (existingUsers > 0)
                {
                    return false;
                }
            }

            const string insertSql = @"
INSERT INTO users (name, password, login, phone, status)
VALUES (@name, @password, @login, @phone, @status);";

            using var insertCommand = new MySqlCommand(insertSql, connection);
            insertCommand.Parameters.AddWithValue("@name", name);
            insertCommand.Parameters.AddWithValue("@password", password);
            insertCommand.Parameters.AddWithValue("@login", login);
            insertCommand.Parameters.AddWithValue("@phone", phone);
            insertCommand.Parameters.AddWithValue("@status", "user");

            return insertCommand.ExecuteNonQuery() > 0;
        }
    }
}