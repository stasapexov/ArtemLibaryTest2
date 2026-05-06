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
SELECT id, name, password, login, phone, status, money, img
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
                reader.GetString("status"),
                reader.IsDBNull(reader.GetOrdinal("money")) ? 0 : reader.GetDouble("money"),
                reader.IsDBNull(reader.GetOrdinal("img")) ? [] : (byte[])reader["img"]);
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
INSERT INTO users (name, password, login, phone, status, money, img)
VALUES (@name, @password, @login, @phone, @status, @money, @img);";

            using var insertCommand = new MySqlCommand(insertSql, connection);
            insertCommand.Parameters.AddWithValue("@name", name);
            insertCommand.Parameters.AddWithValue("@password", password);
            insertCommand.Parameters.AddWithValue("@login", login);
            insertCommand.Parameters.AddWithValue("@phone", phone);
            insertCommand.Parameters.AddWithValue("@status", "user");
            insertCommand.Parameters.AddWithValue("@money", 0);
            // Для схем, где img NOT NULL, сохраняем пустой blob по умолчанию.
            insertCommand.Parameters.AddWithValue("@img", Array.Empty<byte>());

            return insertCommand.ExecuteNonQuery() > 0;
        }

        public bool TopUpUserMoney(int userId, string userPassword, double amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
UPDATE users
SET money = IFNULL(money, 0) + @amount
WHERE id = @id AND password = @password;";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@amount", amount);
            command.Parameters.AddWithValue("@id", userId);
            command.Parameters.AddWithValue("@password", userPassword);

            return command.ExecuteNonQuery() > 0;
        }

        public bool UpdateProfile(int userId, string login, string password, string phone)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
UPDATE users
SET login = @login,
    password = @password,
    phone = @phone
WHERE id = @id;";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@login", login);
            command.Parameters.AddWithValue("@password", password);
            command.Parameters.AddWithValue("@phone", phone);
            command.Parameters.AddWithValue("@id", userId);

            return command.ExecuteNonQuery() > 0;
        }

        public bool UpdateAvatar(int userId, byte[] imageBytes)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            const string sql = @"
UPDATE users
SET img = @img
WHERE id = @id;";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@img", imageBytes);
            command.Parameters.AddWithValue("@id", userId);

            return command.ExecuteNonQuery() > 0;
        }
    }
}
