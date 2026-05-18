using ArtemLibaryTest.Models;
using System.Data;

namespace ArtemLibaryTest.Core
{
    public class MySqlAuthService : IAuthService
    {
        private readonly DbHelper _db;

        public MySqlAuthService(string connectionString)
        {
            _db = new DbHelper(connectionString);
        }

        public Users? Login(string login, string password)
        {
            EnsureUsersEmailColumn();
            EnsureUsersImgColumn();
            var hasEmailColumn = EnsureUsersEmailColumn();
            var emailSelect = hasEmailColumn ? "email" : "'' AS email";

            var sql = $@"
SELECT id, name, password, login, phone, {emailSelect}, status, money, img
FROM users
WHERE login = @login AND password = @password
LIMIT 1;";

            var table = _db.GetTable(
                sql,
                DbHelper.Param("@login", login),
                DbHelper.Param("@password", password));

            if (table.Rows.Count == 0)
            {
                return null;
            }

            var row = table.Rows[0];

            return new Users(
                Convert.ToInt32(row["id"]),
                Convert.ToString(row["name"]) ?? string.Empty,
                Convert.ToString(row["password"]) ?? string.Empty,
                Convert.ToString(row["login"]) ?? string.Empty,
                Convert.ToString(row["phone"]) ?? string.Empty,
                GetStringOrEmpty(row, "email"),
                Convert.ToString(row["status"]) ?? string.Empty,
                row["money"] == DBNull.Value ? 0 : Convert.ToDouble(row["money"]),
                row["img"] == DBNull.Value ? [] : (byte[])row["img"]);
        }

        public void ResetDemoDatabase()
        {
            ResetDemoUsers();
            ResetDemoProducts();
            ResetDemoOrders();
        }

        public void ResetDemoUsers()
        {
            _db.ExecuteNonQuery("DROP TABLE IF EXISTS `users`;");

            const string createUsersTableSql = @"
CREATE TABLE IF NOT EXISTS `users` (
  `id` int NOT NULL AUTO_INCREMENT,
  `login` varchar(25) NOT NULL,
  `password` varchar(25) NOT NULL,
  `name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `inn` int NOT NULL,
  `company_name` varchar(50) NOT NULL,
  `order_id` int NOT NULL,
  `status` varchar(50) NOT NULL,
  `money` decimal(50,0) NOT NULL,
  `img` mediumblob NOT NULL,
  `phone` varchar(25) NOT NULL,
  `email` varchar(50) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `order_id` (`order_id`)
) ENGINE=MyISAM AUTO_INCREMENT=48 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

            _db.ExecuteNonQuery(createUsersTableSql);
            EnsureUsersEmailColumn();

            const string insertDemoUsersSql = @"
INSERT INTO users (id, login, password, name, inn, company_name, order_id, status, money, img, phone, email)
VALUES
    (1, 'artem', '12345', 'Artem', 2147483647, 'Компания', 0, 'admin', 50000, @img, '+719321833', 'letsg527@gmail.com'),
    (49, '1', '1', 'Artem', 2147483647, 'Компания', 0, 'admin', 50000, @img, '+719321833', 'letsg527@gmail.com'),
    (50, '2', '2', 'Artem', 2147483647, 'Компания', 0, 'manager', 50000, @img, '+719321833', 'letsg527@gmail.com'),
    (51, '3', '3', 'Artem', 2147483647, 'Компания', 0, 'user', 50000, @img, '+719321833', 'letsg527@gmail.com');";

            _db.ExecuteNonQuery(insertDemoUsersSql, DbHelper.Param("@img", Array.Empty<byte>()));
        }

        public void ResetDemoProducts()
        {
            _db.ExecuteNonQuery("DROP TABLE IF EXISTS `products`;");

            const string createProductsTableSql = @"
CREATE TABLE IF NOT EXISTS `products` (
  `id` int NOT NULL AUTO_INCREMENT,
  `material_name` varchar(50) NOT NULL,
  `company_name` varchar(50) NOT NULL,
  `quantity` decimal(25,0) NOT NULL,
  `price` decimal(10,0) NOT NULL,
  `photo` varchar(25) NOT NULL DEFAULT 'default.png',
  PRIMARY KEY (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

            _db.ExecuteNonQuery(createProductsTableSql);

            const string insertDemoProductsSql = @"
INSERT INTO products (id, material_name, company_name, quantity, price, photo)
VALUES
    (1, 'Кирпич', 'Компания', 100, 50, 'default.png'),
    (2, 'Цемент', 'Компания', 40, 350, 'default.png'),
    (3, 'Песок', 'Компания', 200, 120, 'default.png'),
    (4, 'Доска', 'Компания', 75, 500, 'default.png');";

            _db.ExecuteNonQuery(insertDemoProductsSql);
        }

        public void ResetDemoOrders()
        {
            _db.ExecuteNonQuery("DROP TABLE IF EXISTS `orders`;");

            const string createOrdersTableSql = @"
CREATE TABLE IF NOT EXISTS `orders` (
  `id` int NOT NULL AUTO_INCREMENT,
  `material_name` varchar(50) NOT NULL,
  `company_name` varchar(50) NOT NULL,
  `date` date NOT NULL,
  `user_id` int NOT NULL,
  `user_name` varchar(50) NOT NULL,
  `quantity` decimal(25,0) NOT NULL,
  `material_id` int NOT NULL,
  `price` decimal(10,0) NOT NULL,
  `photo` varchar(25) NOT NULL DEFAULT 'default.png',
  `readiness` varchar(10) NOT NULL DEFAULT 'не готов',
  PRIMARY KEY (`id`),
  KEY `user_id` (`user_id`),
  KEY `material_id` (`material_id`)
) ENGINE=MyISAM AUTO_INCREMENT=100 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

            _db.ExecuteNonQuery(createOrdersTableSql);

            const string insertDemoOrdersSql = @"
INSERT INTO orders (id, material_name, company_name, date, user_id, user_name, quantity, material_id, price, photo, readiness)
VALUES
    (1, 'Кирпич', 'Компания', '2026-05-11', 49, 'Artem', 10, 1, 500, 'default.png', 'готов'),
    (2, 'Цемент', 'Компания', '2026-05-11', 50, 'Artem', 5, 2, 1750, 'default.png', 'не готов'),
    (3, 'Песок', 'Компания', '2026-05-11', 51, 'Artem', 20, 3, 2400, 'default.png', 'не готов');";

            _db.ExecuteNonQuery(insertDemoOrdersSql);
        }

        public bool Register(string login, string password, string name, string phone, string email = "")
        {
            EnsureUsersImgColumn();
            EnsureUsersEmailColumn();
            var hasEmailColumn = EnsureUsersEmailColumn();

            const string checkSql = "SELECT COUNT(*) FROM users WHERE login = @login;";
            var existingUsers = Convert.ToInt32(_db.ExecuteScalar(checkSql, DbHelper.Param("@login", login)));
            if (existingUsers > 0)
            {
                return false;
            }

            var insertSql = hasEmailColumn
                ? @"
INSERT INTO users (name, password, login, phone, status, money, img, inn, company_name, order_id, email)
VALUES (@name, @password, @login, @phone, @status, @money, @img, @inn, @companyName, @orderId, @email);"
                : @"
INSERT INTO users (name, password, login, phone, status, money, img, inn, company_name, order_id)
VALUES (@name, @password, @login, @phone, @status, @money, @img, @inn, @companyName, @orderId);";

            return _db.ExecuteNonQuery(
                insertSql,
                DbHelper.Param("@name", name),
                DbHelper.Param("@password", password),
                DbHelper.Param("@login", login),
                DbHelper.Param("@phone", phone),
                DbHelper.Param("@status", "user"),
                DbHelper.Param("@money", 0),
                // Для схем, где img NOT NULL, сохраняем пустой blob по умолчанию.
                DbHelper.Param("@img", Array.Empty<byte>()),
                // Дефолты для демо-схемы users, чтобы регистрация продолжала работать после быстрого сброса БД.
                DbHelper.Param("@inn", 0),
                DbHelper.Param("@companyName", string.Empty),
                DbHelper.Param("@orderId", 0),
                DbHelper.Param("@email", email)) > 0;
        }

        public bool TopUpUserMoney(int userId, string userPassword, double amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            const string sql = @"
UPDATE users
SET money = IFNULL(money, 0) + @amount
WHERE id = @id AND password = @password;";

            return _db.ExecuteNonQuery(
                sql,
                DbHelper.Param("@amount", amount),
                DbHelper.Param("@id", userId),
                DbHelper.Param("@password", userPassword)) > 0;
        }
        private void EnsureUsersImgColumn()
        {
            const string checkColumnSql = @"
SELECT COUNT(*)
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'users'
  AND COLUMN_NAME = 'img';";

            var imgColumnExists = Convert.ToInt32(_db.ExecuteScalar(checkColumnSql)) > 0;
            if (!imgColumnExists)
            {
                return;
            }

            const string checkTypeSql = @"
SELECT DATA_TYPE
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'users'
  AND COLUMN_NAME = 'img'
LIMIT 1;";

            var dataType = Convert.ToString(_db.ExecuteScalar(checkTypeSql));
            if (string.Equals(dataType, "mediumblob", StringComparison.OrdinalIgnoreCase)
                || string.Equals(dataType, "longblob", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _db.ExecuteNonQuery("ALTER TABLE `users` MODIFY COLUMN `img` MEDIUMBLOB NOT NULL;");
        }

        public bool UpdateProfile(int userId, string login, string password, string phone, string email)
        {
            var hasEmailColumn = EnsureUsersEmailColumn();
            var emailUpdate = hasEmailColumn ? ",\n    email = @email" : string.Empty;

            var sql = $@"
UPDATE users
SET login = @login,
    password = @password,
    phone = @phone{emailUpdate}
WHERE id = @id;";

            return _db.ExecuteNonQuery(
                sql,
                DbHelper.Param("@login", login),
                DbHelper.Param("@password", password),
                DbHelper.Param("@phone", phone),
                DbHelper.Param("@email", email),
                DbHelper.Param("@id", userId)) > 0;
        }

        private bool EnsureUsersEmailColumn()
        {
            const string checkTableSql = @"
SELECT COUNT(*)
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'users';";

            var usersTableExists = Convert.ToInt32(_db.ExecuteScalar(checkTableSql)) > 0;
            if (!usersTableExists)
            {
                return false;
            }

            if (UsersEmailColumnExists())
            {
                return true;
            }

            _db.ExecuteNonQuery("ALTER TABLE `users` ADD COLUMN `email` varchar(50) NOT NULL DEFAULT '';");
            return UsersEmailColumnExists();
        }

        private bool UsersEmailColumnExists()
        {
            const string checkColumnSql = @"
SELECT COUNT(*)
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'users'
  AND COLUMN_NAME = 'email';";

            return Convert.ToInt32(_db.ExecuteScalar(checkColumnSql)) > 0;
        }

        private static string GetStringOrEmpty(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return string.Empty;
            }

            return Convert.ToString(row[columnName]) ?? string.Empty;
        }

        public bool UpdateAvatar(int userId, byte[] imageBytes)
        {
            EnsureUsersImgColumn();
            const string sql = @"
UPDATE users
SET img = @img
WHERE id = @id;";

            return _db.ExecuteNonQuery(
                sql,
                DbHelper.Param("@img", imageBytes),
                DbHelper.Param("@id", userId)) > 0;
        }
    }
}