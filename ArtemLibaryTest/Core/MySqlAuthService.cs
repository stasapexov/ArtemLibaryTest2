using ArtemLibaryTest.Models;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Text;

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
            _db.ExecuteNonQuery("DROP TABLE IF EXISTS `order_items`;");
            _db.ExecuteNonQuery("DROP TABLE IF EXISTS `payments`;");
            _db.ExecuteNonQuery("DROP TABLE IF EXISTS `addresses`;");
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
  PRIMARY KEY (`id`),
  UNIQUE KEY `ux_users_login` (`login`)
) ENGINE=InnoDB AUTO_INCREMENT=48 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

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
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

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
) ENGINE=InnoDB AUTO_INCREMENT=100 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

            const string insertOrderItemsSql = @"
INSERT INTO order_items (order_id, product_id, quantity, unit_price)
VALUES
    (1, 1, 10, 50),
    (2, 2, 5, 350),
    (3, 3, 20, 120);";

            const string createAddressesSql = @"
CREATE TABLE IF NOT EXISTS `addresses` (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `city` varchar(100) NOT NULL,
  `street` varchar(100) NOT NULL,
  `house` varchar(20) NOT NULL,
  `postal_code` varchar(20) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_addresses_user_id` (`user_id`),
  CONSTRAINT `fk_addresses_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

            const string insertPaymentsSql = @"
INSERT INTO payments (order_id, amount, payment_method, status, paid_at)
VALUES
    (1, 500, 'card', 'paid', '2026-05-11 10:00:00'),
    (2, 1750, 'bank_transfer', 'pending', NULL),
    (3, 2400, 'cash', 'pending', NULL);";

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
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

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
) ENGINE=InnoDB AUTO_INCREMENT=100 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

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
            _db.ExecuteNonQuery(createAddressesSql);
            _db.ExecuteNonQuery(createPaymentsSql);
            _db.ExecuteNonQuery(insertPaymentsSql);
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

        public DataTable GetStoreProducts(string? name, decimal? minPrice, decimal? maxPrice, int? categoryId)
        {
            var sql = @"
SELECT p.id, p.name, c.name AS category, p.quantity, p.price, p.photo
FROM products p
LEFT JOIN categories c ON c.id = p.category_id
WHERE 1=1";

            var parameters = new List<MySqlParameter>();
            var builder = new StringBuilder(sql);
            DbHelper.AddWhereLike(builder, parameters, "p.name", "@name", name);
            DbHelper.AddWhereMin(builder, parameters, "p.price", "@minPrice", minPrice.HasValue ? Convert.ToDouble(minPrice.Value) : null);
            DbHelper.AddWhereMax(builder, parameters, "p.price", "@maxPrice", maxPrice.HasValue ? Convert.ToDouble(maxPrice.Value) : null);
            if (categoryId.HasValue)
            {
                builder.Append(" AND p.category_id = @categoryId");
                parameters.Add(DbHelper.Param("@categoryId", categoryId.Value));
            }

            builder.Append(" ORDER BY p.name;");
            return _db.GetTable(builder.ToString(), parameters.ToArray());
        }

        public DataTable GetCategories()
        {
            return _db.GetTable("SELECT id, name FROM categories ORDER BY name;");
        }

        public bool CreateOrder(int userId, int productId, decimal quantity)
        {
            if (quantity <= 0)
            {
                return false;
            }

            const string getProductSql = @"
SELECT id, price, quantity
FROM products
WHERE id = @productId
LIMIT 1;";

            var product = _db.GetTable(getProductSql, DbHelper.Param("@productId", productId));
            if (product.Rows.Count == 0)
            {
                return false;
            }

            var available = Convert.ToDecimal(product.Rows[0]["quantity"]);
            if (available < quantity)
            {
                return false;
            }

            var price = Convert.ToDecimal(product.Rows[0]["price"]);
            var totalPrice = price * quantity;

            const string insertOrderSql = @"
INSERT INTO orders (`date`, user_id, total_price, readiness)
VALUES (CURDATE(), @userId, @totalPrice, 'не готов');";
            _db.ExecuteNonQuery(insertOrderSql, DbHelper.Param("@userId", userId), DbHelper.Param("@totalPrice", totalPrice));

            var orderId = Convert.ToInt32(_db.ExecuteScalar("SELECT LAST_INSERT_ID();"));

            const string insertItemSql = @"
INSERT INTO order_items (order_id, product_id, quantity, unit_price)
VALUES (@orderId, @productId, @quantity, @unitPrice);";
            _db.ExecuteNonQuery(
                insertItemSql,
                DbHelper.Param("@orderId", orderId),
                DbHelper.Param("@productId", productId),
                DbHelper.Param("@quantity", quantity),
                DbHelper.Param("@unitPrice", price));

            const string decreaseSql = @"
UPDATE products
SET quantity = quantity - @quantity
WHERE id = @productId;";
            _db.ExecuteNonQuery(decreaseSql, DbHelper.Param("@quantity", quantity), DbHelper.Param("@productId", productId));

            return true;
        }

        public DataTable GetMyOrders(int userId)
        {
            const string sql = @"
SELECT o.id, o.date, o.total_price, o.readiness,
       p.name AS product_name, oi.quantity, oi.unit_price
FROM orders o
JOIN order_items oi ON oi.order_id = o.id
JOIN products p ON p.id = oi.product_id
WHERE o.user_id = @userId
ORDER BY o.id DESC;";
            return _db.GetTable(sql, DbHelper.Param("@userId", userId));
        }
    }
}
