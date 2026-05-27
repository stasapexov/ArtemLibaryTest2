SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS `product_characteristics`;
DROP TABLE IF EXISTS `employees`;
DROP TABLE IF EXISTS `order_items`;
DROP TABLE IF EXISTS `payments`;
DROP TABLE IF EXISTS `orders`;
DROP TABLE IF EXISTS `products`;
DROP TABLE IF EXISTS `categories`;
DROP TABLE IF EXISTS `users`;
SET FOREIGN_KEY_CHECKS = 1;

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
) ENGINE=InnoDB AUTO_INCREMENT=48 DEFAULT CHARSET=utf8mb4;

INSERT INTO users (id, login, password, name, status, money, img, phone, email)
VALUES
    (1, 'artem', '12345', 'Artem', 'admin', 50000, X'', '+719321833', 'letsg527@gmail.com'),
    (49, '1', '1', 'Artem', 'admin', 50000, X'', '+719321833', 'letsg527@gmail.com'),
    (50, '2', '2', 'Artem', 'manager', 50000, X'', '+719321833', 'letsg527@gmail.com'),
    (51, '3', '3', 'Artem', 'user', 50000, X'', '+719321833', 'letsg527@gmail.com');

CREATE TABLE IF NOT EXISTS `categories` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `ux_categories_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO categories (id, name)
VALUES (1, 'Строительные смеси'), (2, 'Сыпучие материалы'), (3, 'Пиломатериалы');

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
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4;

INSERT INTO products (id, name, category_id, quantity, price, photo)
VALUES
    (1, 'Кирпич', 1, 100, 50, 'default.png'),
    (2, 'Цемент', 1, 40, 350, 'default.png'),
    (3, 'Песок', 2, 200, 120, 'default.png'),
    (4, 'Доска', 3, 75, 500, 'default.png');

CREATE TABLE IF NOT EXISTS `orders` (
  `id` int NOT NULL AUTO_INCREMENT,
  `date` date NOT NULL,
  `user_id` int NOT NULL,
  `total_price` decimal(10,2) NOT NULL,
  `readiness` varchar(10) NOT NULL DEFAULT 'не готов',
  PRIMARY KEY (`id`),
  KEY `idx_orders_user_id` (`user_id`),
  CONSTRAINT `fk_orders_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=100 DEFAULT CHARSET=utf8mb4;

INSERT INTO orders (id, date, user_id, total_price, readiness)
VALUES
    (1, '2026-05-11', 49, 500, 'готов'),
    (2, '2026-05-11', 50, 1750, 'не готов'),
    (3, '2026-05-11', 51, 2400, 'не готов');

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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO order_items (order_id, product_id, quantity, unit_price)
VALUES
    (1, 1, 10, 50),
    (2, 2, 5, 350),
    (3, 3, 20, 120);

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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO payments (order_id, amount, payment_method, status, paid_at)
VALUES
    (1, 500, 'card', 'paid', '2026-05-11 10:00:00'),
    (2, 1750, 'bank_transfer', 'pending', NULL),
    (3, 2400, 'cash', 'pending', NULL);

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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO employees (id, user_id, full_name, position, hire_date, salary)
VALUES
    (1, 49, 'Артем Администратор', 'Администратор', '2025-01-15', 85000),
    (2, 50, 'Артем Менеджер', 'Менеджер по продажам', '2025-04-02', 65000),
    (3, NULL, 'Иван Кладовщик', 'Кладовщик', '2024-10-20', 55000);

CREATE TABLE IF NOT EXISTS `product_characteristics` (
  `id` int NOT NULL AUTO_INCREMENT,
  `product_id` int NOT NULL,
  `name` varchar(120) NOT NULL,
  `value` varchar(500) NOT NULL,
  `display_order` int NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  KEY `idx_product_characteristics_product_id` (`product_id`),
  CONSTRAINT `fk_product_characteristics_product` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO product_characteristics (product_id, name, value, display_order)
VALUES
    (1, 'Марка прочности', 'М150', 1),
    (1, 'Размер', '250x120x65 мм', 2),
    (2, 'Класс', 'ПЦ 500 Д0', 1),
    (2, 'Вес мешка', '50 кг', 2),
    (3, 'Фракция', '0.5-2.0 мм', 1),
    (4, 'Порода древесины', 'Сосна', 1),
    (4, 'Влажность', 'до 18%', 2);
