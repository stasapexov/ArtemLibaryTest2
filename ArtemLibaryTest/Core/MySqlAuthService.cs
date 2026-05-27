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
            _db.ExecuteNonQuery("DROP TABLE IF EXISTS `product_characteristics`;");
            _db.ExecuteNonQuery("DROP TABLE IF EXISTS `employees`;");
            _db.ExecuteNonQuery("DROP TABLE IF EXISTS `order_items`;");
            _db.ExecuteNonQuery("DROP TABLE IF EXISTS `payments`;");
            _db.ExecuteNonQuery("DROP TABLE IF EXISTS `categories`;");
            _db.ExecuteNonQuery("DROP TABLE IF EXISTS `orders`;");
            _db.ExecuteNonQuery("DROP TABLE IF EXISTS `products`;");
            _db.ExecuteNonQuery("DROP TABLE IF EXISTS `users`;");

            ResetDemoUsers();
            ResetDemoStoreTables();
        }

        public void ResetDemoUsers()
        {
            _db.ExecuteNonQuery("DROP TABLE IF EXISTS `users`;");

            const string createUsersTableSql = @"
CREATE TABLE IF NOT EXISTS `users` (
  `id` int NOT NULL AUTO_INCREMENT,
  `login` varchar(25) NOT NULL,
  `password` varchar(25) NOT NULL,
  `name` varchar(50) NOT NULL,
  `status` varchar(50) NOT NULL,
  `money` decimal(12,2) NOT NULL DEFAULT 0,
  `img` mediumblob NOT NULL,
  `phone` varchar(25) NOT NULL,
  `email` varchar(50) NOT NULL,
  `city` varchar(100) NOT NULL DEFAULT '',
  `street` varchar(100) NOT NULL DEFAULT '',
  `house` varchar(20) NOT NULL DEFAULT '',
  PRIMARY KEY (`id`),
  UNIQUE KEY `ux_users_login` (`login`)
) ENGINE=InnoDB AUTO_INCREMENT=48 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;";

            _db.ExecuteNonQuery(createUsersTableSql);

            const string insertDemoUsersSql = @"
INSERT INTO users (id, login, password, name, status, money, img, phone, email)
VALUES
    (1, 'artem', '12345', 'Artem', 'admin', 50000, @img, '+719321833', 'letsg527@gmail.com'),
    (49, '1', '1', 'Artem', 'admin', 50000, @img, '+719321833', 'letsg527@gmail.com'),
    (50, '2', '2', 'Artem', 'manager', 50000, @img, '+719321833', 'letsg527@gmail.com'),
    (51, '3', '3', 'Artem', 'user', 50000, @img, '+719321833', 'letsg527@gmail.com');";

            _db.ExecuteNonQuery(insertDemoUsersSql, DbHelper.Param("@img", Array.Empty<byte>()));
        }

        public void ResetDemoProducts()
        {
            _db.ExecuteNonQuery("DROP TABLE IF EXISTS `products`;");

            const string createProductsTableSql = @"
CREATE TABLE IF NOT EXISTS `products` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `category_id` int NULL,
  `quantity` decimal(25,0) NOT NULL,
  `price` decimal(10,2) NOT NULL,
  `photo` varchar(100) NOT NULL DEFAULT 'default.png',
  PRIMARY KEY (`id`),
  KEY `idx_products_category_id` (`category_id`),
  CONSTRAINT `fk_products_category` FOREIGN KEY (`category_id`) REFERENCES `categories` (`id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;";

            _db.ExecuteNonQuery(createProductsTableSql);

            const string insertDemoProductsSql = @"
INSERT INTO products (id, name, category_id, quantity, price, photo)
VALUES
    (1, 'Кирпич', 1, 100, 50, 'default.png'),
    (2, 'Цемент', 1, 40, 350, 'default.png'),
    (3, 'Песок', 2, 200, 120, 'default.png'),
    (4, 'Доска', 3, 75, 500, 'default.png');";

            _db.ExecuteNonQuery(insertDemoProductsSql);
        }

        public void ResetDemoOrders()
        {
            _db.ExecuteNonQuery("DROP TABLE IF EXISTS `orders`;");

            const string createOrdersTableSql = @"
CREATE TABLE IF NOT EXISTS `orders` (
  `id` int NOT NULL AUTO_INCREMENT,
  `date` date NOT NULL,
  `user_id` int NOT NULL,
  `total_price` decimal(10,2) NOT NULL,
  `readiness` varchar(10) NOT NULL DEFAULT 'не готов',
  PRIMARY KEY (`id`),
  KEY `idx_orders_user_id` (`user_id`),
  CONSTRAINT `fk_orders_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=100 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;";

            _db.ExecuteNonQuery(createOrdersTableSql);

            const string insertDemoOrdersSql = @"
INSERT INTO orders (id, date, user_id, total_price, readiness)
VALUES
    (1, '2026-05-11', 49, 500, 'готов'),
    (2, '2026-05-11', 50, 1750, 'не готов'),
    (3, '2026-05-11', 51, 2400, 'не готов');";

            _db.ExecuteNonQuery(insertDemoOrdersSql);
        }

        public void ResetDemoStoreTables()
        {
            const string createCategoriesSql = @"
CREATE TABLE IF NOT EXISTS `categories` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `ux_categories_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;";

            const string insertCategoriesSql = @"
INSERT INTO categories (id, name)
VALUES (1, 'Строительные смеси'), (2, 'Сыпучие материалы'), (3, 'Пиломатериалы');";

            const string createOrderItemsSql = @"
CREATE TABLE IF NOT EXISTS `order_items` (
  `id` int NOT NULL AUTO_INCREMENT,
  `order_id` int NOT NULL,
  `product_id` int NOT NULL,
  `quantity` decimal(25,0) NOT NULL,
  `unit_price` decimal(10,2) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_order_items_order_id` (`order_id`),
  KEY `idx_order_items_product_id` (`product_id`),
  CONSTRAINT `fk_order_items_order` FOREIGN KEY (`order_id`) REFERENCES `orders` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_order_items_product` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;";

            const string insertOrderItemsSql = @"
INSERT INTO order_items (order_id, product_id, quantity, unit_price)
VALUES
    (1, 1, 10, 50),
    (2, 2, 5, 350),
    (3, 3, 20, 120);";

            const string createPaymentsSql = @"
CREATE TABLE IF NOT EXISTS `payments` (
  `id` int NOT NULL AUTO_INCREMENT,
  `order_id` int NOT NULL,
  `amount` decimal(10,2) NOT NULL,
  `payment_method` varchar(50) NOT NULL,
  `status` varchar(30) NOT NULL,
  `paid_at` datetime NULL,
  PRIMARY KEY (`id`),
  KEY `idx_payments_order_id` (`order_id`),
  CONSTRAINT `fk_payments_order` FOREIGN KEY (`order_id`) REFERENCES `orders` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;";

            const string insertPaymentsSql = @"
INSERT INTO payments (order_id, amount, payment_method, status, paid_at)
VALUES
    (1, 500, 'card', 'paid', '2026-05-11 10:00:00'),
    (2, 1750, 'bank_transfer', 'pending', NULL),
    (3, 2400, 'cash', 'pending', NULL);";

            const string createEmployeesSql = @"
CREATE TABLE IF NOT EXISTS `employees` (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NULL,
  `full_name` varchar(120) NOT NULL,
  `position` varchar(80) NOT NULL,
  `hire_date` date NOT NULL,
  `salary` decimal(12,2) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_employees_user_id` (`user_id`),
  CONSTRAINT `fk_employees_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;";

            const string insertEmployeesSql = @"
INSERT INTO employees (id, user_id, full_name, position, hire_date, salary)
VALUES
    (1, 49, 'Артем Администратор', 'Администратор', '2025-01-15', 85000),
    (2, 50, 'Артем Менеджер', 'Менеджер по продажам', '2025-04-02', 65000),
    (3, NULL, 'Иван Кладовщик', 'Кладовщик', '2024-10-20', 55000);";

            const string createProductCharacteristicsSql = @"
CREATE TABLE IF NOT EXISTS `product_characteristics` (
  `id` int NOT NULL AUTO_INCREMENT,
  `product_id` int NOT NULL,
  `name` varchar(120) NOT NULL,
  `value` varchar(500) NOT NULL,
  `display_order` int NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  KEY `idx_product_characteristics_product_id` (`product_id`),
  CONSTRAINT `fk_product_characteristics_product` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;";

            const string insertProductCharacteristicsSql = @"
INSERT INTO product_characteristics (product_id, name, value, display_order)
VALUES
    (1, 'Марка прочности', 'М150', 1),
    (1, 'Размер', '250x120x65 мм', 2),
    (2, 'Класс', 'ПЦ 500 Д0', 1),
    (2, 'Вес мешка', '50 кг', 2),
    (3, 'Фракция', '0.5-2.0 мм', 1),
    (4, 'Порода древесины', 'Сосна', 1),
    (4, 'Влажность', 'до 18%', 2);";

            const string createProductsTableSql = @"
CREATE TABLE IF NOT EXISTS `products` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `category_id` int NULL,
  `quantity` decimal(25,0) NOT NULL,
  `price` decimal(10,2) NOT NULL,
  `photo` varchar(100) NOT NULL DEFAULT 'default.png',
  PRIMARY KEY (`id`),
  KEY `idx_products_category_id` (`category_id`),
  CONSTRAINT `fk_products_category` FOREIGN KEY (`category_id`) REFERENCES `categories` (`id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;";

            const string insertDemoProductsSql = @"
INSERT INTO products (id, name, category_id, quantity, price, photo)
VALUES
    (1, 'Кирпич', 1, 100, 50, 'default.png'),
    (2, 'Цемент', 1, 40, 350, 'default.png'),
    (3, 'Песок', 2, 200, 120, 'default.png'),
    (4, 'Доска', 3, 75, 500, 'default.png');";

            const string createOrdersTableSql = @"
CREATE TABLE IF NOT EXISTS `orders` (
  `id` int NOT NULL AUTO_INCREMENT,
  `date` date NOT NULL,
  `user_id` int NOT NULL,
  `total_price` decimal(10,2) NOT NULL,
  `readiness` varchar(10) NOT NULL DEFAULT 'не готов',
  PRIMARY KEY (`id`),
  KEY `idx_orders_user_id` (`user_id`),
  CONSTRAINT `fk_orders_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=100 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;";

            const string insertDemoOrdersSql = @"
INSERT INTO orders (id, date, user_id, total_price, readiness)
VALUES
    (1, '2026-05-11', 49, 500, 'готов'),
    (2, '2026-05-11', 50, 1750, 'не готов'),
    (3, '2026-05-11', 51, 2400, 'не готов');";

            _db.ExecuteNonQuery(createCategoriesSql);
            _db.ExecuteNonQuery(insertCategoriesSql);
            _db.ExecuteNonQuery(createProductsTableSql);
            _db.ExecuteNonQuery(insertDemoProductsSql);
            _db.ExecuteNonQuery(createOrdersTableSql);
            _db.ExecuteNonQuery(insertDemoOrdersSql);
            _db.ExecuteNonQuery(createOrderItemsSql);
            _db.ExecuteNonQuery(insertOrderItemsSql);
            _db.ExecuteNonQuery(createPaymentsSql);
            _db.ExecuteNonQuery(insertPaymentsSql);
            _db.ExecuteNonQuery(createEmployeesSql);
            _db.ExecuteNonQuery(insertEmployeesSql);
            _db.ExecuteNonQuery(createProductCharacteristicsSql);
            _db.ExecuteNonQuery(insertProductCharacteristicsSql);
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
INSERT INTO users (name, password, login, phone, status, money, img, email)
VALUES (@name, @password, @login, @phone, @status, @money, @img, @email);"
                : @"
INSERT INTO users (name, password, login, phone, status, money, img)
VALUES (@name, @password, @login, @phone, @status, @money, @img);";

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
