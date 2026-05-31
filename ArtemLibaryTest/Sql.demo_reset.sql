SET NAMES utf8;
SET FOREIGN_KEY_CHECKS = 0;

DROP VIEW IF EXISTS `v_orders`;
DROP VIEW IF EXISTS `v_products`;
DROP VIEW IF EXISTS `v_categories`;

DROP TABLE IF EXISTS `employees`;
DROP TABLE IF EXISTS `orders`;
DROP TABLE IF EXISTS `pickup_points`;
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
  PRIMARY KEY (`id`),
  UNIQUE KEY `ux_users_login` (`login`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;

INSERT INTO `users` 
(`id`, `login`, `password`, `name`, `status`, `money`, `img`, `phone`, `email`)
VALUES
  (1, 'artem', '12345', 'Artem', 'admin', 50000.00, '', '+79990000001', 'admin@furniture-shop.local'),
  (49, '1', '1', 'Artem', 'admin', 50000.00, '', '+79990000002', 'admin.demo@furniture-shop.local'),
  (50, '2', '2', 'Artem', 'manager', 30000.00, '', '+79990000003', 'manager@furniture-shop.local'),
  (51, '3', '3', 'Artem', 'user', 120000.00, '', '+79990000004', 'user@furniture-shop.local');

CREATE TABLE IF NOT EXISTS `categories` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `ux_categories_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;


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
  CONSTRAINT `fk_products_category`
    FOREIGN KEY (`category_id`)
    REFERENCES `categories` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;


CREATE TABLE IF NOT EXISTS `pickup_points` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `address` varchar(255) NOT NULL DEFAULT '',
  `phone` varchar(25) NOT NULL DEFAULT '',
  `working_hours` varchar(100) NOT NULL DEFAULT '',
  PRIMARY KEY (`id`),
  KEY `idx_pickup_points_address` (`address`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;


CREATE TABLE IF NOT EXISTS `orders` (
  `id` int NOT NULL AUTO_INCREMENT,
  `date` date NOT NULL,
  `user_id` int NOT NULL,
  `product_id` int NOT NULL,
  `product_name` varchar(100) NOT NULL,
  `product_photo` varchar(100) NOT NULL DEFAULT 'default.png',
  `product_material` varchar(120) NOT NULL DEFAULT '',
  `product_color` varchar(80) NOT NULL DEFAULT '',
  `product_dimensions` varchar(80) NOT NULL DEFAULT '',
  `quantity` int NOT NULL DEFAULT 1,
  `unit_price` decimal(10,2) NOT NULL DEFAULT 0,
  `total_price` decimal(10,2) NOT NULL DEFAULT 0,
  `readiness` varchar(20) NOT NULL DEFAULT 'New',
  `pickup_point_id` int DEFAULT NULL,
  `pickup_address` varchar(255) NOT NULL DEFAULT '',
  `pickup_code` varchar(12) NOT NULL DEFAULT '',
  PRIMARY KEY (`id`),
  KEY `idx_orders_user_id` (`user_id`),
  KEY `idx_orders_product_id` (`product_id`),
  KEY `idx_orders_product_name` (`product_name`),
  KEY `idx_orders_pickup_point_id` (`pickup_point_id`),
  KEY `idx_orders_pickup_code` (`pickup_code`),
  CONSTRAINT `fk_orders_user`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_orders_product`
    FOREIGN KEY (`product_id`)
    REFERENCES `products` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_orders_pickup_point`
    FOREIGN KEY (`pickup_point_id`)
    REFERENCES `pickup_points` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;


CREATE VIEW `v_categories` AS
SELECT
  id,
  name
FROM categories;

CREATE VIEW `v_products` AS
SELECT
  products.id,
  products.name,
  products.category_id,
  categories.name AS category_name,
  products.quantity,
  products.price,
  products.photo,
  products.material,
  products.color,
  products.dimensions,
  products.description
FROM products
LEFT JOIN categories ON categories.id = products.category_id;

CREATE VIEW `v_orders` AS
SELECT
  orders.id,
  orders.date,
  orders.user_id,
  users.login,
  users.name AS user_name,
  orders.product_id,
  orders.product_name,
  orders.product_photo,
  orders.product_material,
  orders.product_color,
  orders.product_dimensions,
  orders.quantity,
  orders.unit_price,
  orders.total_price,
  orders.readiness
FROM orders
INNER JOIN users ON users.id = orders.user_id;

CREATE TABLE IF NOT EXISTS `employees` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `position` varchar(100) NOT NULL,
  `phone` varchar(25) NOT NULL,
  `email` varchar(50) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;
