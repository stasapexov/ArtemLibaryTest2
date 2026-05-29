SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS `order_items`;
DROP TABLE IF EXISTS `product_characteristics`;
DROP TABLE IF EXISTS `employees`;
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `users` (`id`, `login`, `password`, `name`, `status`, `money`, `img`, `phone`, `email`, `city`, `street`, `house`)
VALUES
  (1, 'artem', '12345', 'Артем', 'admin', 50000.00, '', '+79990000001', 'admin@furniture-shop.local', 'Москва', 'Лесная', '1'),
  (49, '1', '1', 'Администратор', 'admin', 50000.00, '', '+79990000002', 'admin.demo@furniture-shop.local', 'Москва', 'Лесная', '1'),
  (50, '2', '2', 'Менеджер', 'manager', 30000.00, '', '+79990000003', 'manager@furniture-shop.local', 'Москва', 'Складская', '7'),
  (51, '3', '3', 'Покупатель', 'user', 120000.00, '', '+79990000004', 'user@furniture-shop.local', 'Казань', 'Домашняя', '12');

CREATE TABLE IF NOT EXISTS `categories` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `ux_categories_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `products` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `category_id` int DEFAULT NULL,
  `quantity` int NOT NULL DEFAULT 0,
  `price` decimal(10,2) NOT NULL DEFAULT 0,
  `photo` varchar(100) NOT NULL DEFAULT 'default.png',
  `material` varchar(120) NOT NULL DEFAULT '',
  `color` varchar(80) NOT NULL DEFAULT '',
  `dimensions` varchar(80) NOT NULL DEFAULT '',
  `description` varchar(500) NOT NULL DEFAULT '',
  PRIMARY KEY (`id`),
  KEY `idx_products_category_id` (`category_id`),
  CONSTRAINT `fk_products_category` FOREIGN KEY (`category_id`) REFERENCES `categories` (`id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `orders` (
  `id` int NOT NULL AUTO_INCREMENT,
  `date` date NOT NULL,
  `user_id` int NOT NULL,
  `product_id` int NOT NULL,
  `product_name` varchar(100) NOT NULL,
  `product_material` varchar(120) NOT NULL DEFAULT '',
  `product_color` varchar(80) NOT NULL DEFAULT '',
  `product_dimensions` varchar(80) NOT NULL DEFAULT '',
  `quantity` int NOT NULL DEFAULT 1,
  `unit_price` decimal(10,2) NOT NULL DEFAULT 0,
  `total_price` decimal(10,2) NOT NULL DEFAULT 0,
  `readiness` varchar(20) NOT NULL DEFAULT 'Новый',
  PRIMARY KEY (`id`),
  KEY `idx_orders_user_id` (`user_id`),
  KEY `idx_orders_product_id` (`product_id`),
  KEY `idx_orders_product_name` (`product_name`),
  CONSTRAINT `fk_orders_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

