SET NAMES utf8;
SET FOREIGN_KEY_CHECKS = 0;

DROP VIEW IF EXISTS `v_order_items_products`;
DROP VIEW IF EXISTS `v_products_characteristics`;
DROP VIEW IF EXISTS `v_order_details`;

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
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;

INSERT INTO `users` 
(`id`, `login`, `password`, `name`, `status`, `money`, `img`, `phone`, `email`, `city`, `street`, `house`)
VALUES
  (1, 'artem', '12345', 'Artem', 'admin', 50000.00, '', '+79990000001', 'admin@furniture-shop.local', 'Moscow', 'Pushkina', '1'),
  (49, '1', '1', 'Artem', 'admin', 50000.00, '', '+79990000002', 'admin.demo@furniture-shop.local', 'Moscow', 'Pushkina', '1'),
  (50, '2', '2', 'Artem', 'manager', 30000.00, '', '+79990000003', 'manager@furniture-shop.local', 'Moscow', 'Pushkina', '7'),
  (51, '3', '3', 'Artem', 'user', 120000.00, '', '+79990000004', 'user@furniture-shop.local', 'Moscow', 'Pushkina', '12');

CREATE TABLE IF NOT EXISTS `categories` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `ux_categories_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;

INSERT INTO `categories` (`id`, `name`)
VALUES
  (1, 'Столы'),
  (2, 'Стулья'),
  (3, 'Шкафы'),
  (4, 'Диваны');

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

INSERT INTO `products`
(`id`, `name`, `category_id`, `quantity`, `price`, `photo`, `material`, `color`, `dimensions`, `description`)
VALUES
  (1, 'Письменный стол Loft', 1, 12, 8900.00, 'desk_loft.png', 'ЛДСП, металл', 'Дуб вотан / чёрный', '120x60x75 см', 'Компактный стол для учёбы и работы.'),
  (2, 'Обеденный стол Family', 1, 8, 14500.00, 'table_family.png', 'Массив сосны', 'Орех', '160x90x76 см', 'Большой стол для кухни или гостиной.'),
  (3, 'Стул Soft', 2, 24, 3200.00, 'chair_soft.png', 'Металл, ткань', 'Серый', '45x52x86 см', 'Мягкий стул с устойчивым металлическим каркасом.'),
  (4, 'Шкаф Classic', 3, 6, 21900.00, 'wardrobe_classic.png', 'ЛДСП', 'Белый', '180x60x220 см', 'Распашной шкаф с полками и штангой.'),
  (5, 'Диван Comfort', 4, 4, 34900.00, 'sofa_comfort.png', 'Велюр, фанера', 'Синий', '210x95x90 см', 'Раскладной диван для ежедневного отдыха.');

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
  `readiness` varchar(20) NOT NULL DEFAULT 'New',
  PRIMARY KEY (`id`),
  KEY `idx_orders_user_id` (`user_id`),
  KEY `idx_orders_product_id` (`product_id`),
  KEY `idx_orders_product_name` (`product_name`),
  CONSTRAINT `fk_orders_user`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_orders_product`
    FOREIGN KEY (`product_id`)
    REFERENCES `products` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;

INSERT INTO `orders`
(`id`, `date`, `user_id`, `product_id`, `product_name`, `product_material`, `product_color`, `product_dimensions`, `quantity`, `unit_price`, `total_price`, `readiness`)
VALUES
  (1, CURRENT_DATE, 51, 1, 'Письменный стол Loft', 'ЛДСП, металл', 'Дуб вотан / чёрный', '120x60x75 см', 1, 8900.00, 8900.00, 'New'),
  (2, CURRENT_DATE, 51, 3, 'Стул Soft', 'Металл, ткань', 'Серый', '45x52x86 см', 2, 3200.00, 6400.00, 'InProgress');

CREATE TABLE IF NOT EXISTS `order_items` (
  `id` int NOT NULL AUTO_INCREMENT,
  `order_id` int NOT NULL,
  `product_id` int NOT NULL,
  `quantity` int NOT NULL DEFAULT 1,
  `price` decimal(10,2) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  KEY `idx_order_items_order_id` (`order_id`),
  KEY `idx_order_items_product_id` (`product_id`),
  CONSTRAINT `fk_order_items_order`
    FOREIGN KEY (`order_id`)
    REFERENCES `orders` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_order_items_product`
    FOREIGN KEY (`product_id`)
    REFERENCES `products` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;

INSERT INTO `order_items`
(`id`, `order_id`, `product_id`, `quantity`, `price`)
VALUES
  (1, 1, 1, 1, 8900.00),
  (2, 2, 3, 2, 3200.00);

CREATE TABLE IF NOT EXISTS `product_characteristics` (
  `id` int NOT NULL AUTO_INCREMENT,
  `product_id` int NOT NULL,
  `name` varchar(100) NOT NULL,
  `value` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `ux_product_characteristics_product_name` (`product_id`, `name`),
  KEY `idx_product_characteristics_product_id` (`product_id`),
  CONSTRAINT `fk_product_characteristics_product`
    FOREIGN KEY (`product_id`)
    REFERENCES `products` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;

INSERT INTO `product_characteristics` (`product_id`, `name`, `value`)
VALUES
  (1, 'Материал', 'ЛДСП, металл'),
  (1, 'Цвет', 'Дуб вотан / чёрный'),
  (1, 'Размеры', '120x60x75 см'),
  (2, 'Материал', 'Массив сосны'),
  (2, 'Цвет', 'Орех'),
  (2, 'Размеры', '160x90x76 см'),
  (3, 'Материал', 'Металл, ткань'),
  (3, 'Цвет', 'Серый'),
  (3, 'Размеры', '45x52x86 см'),
  (4, 'Материал', 'ЛДСП'),
  (4, 'Цвет', 'Белый'),
  (4, 'Размеры', '180x60x220 см'),
  (5, 'Материал', 'Велюр, фанера'),
  (5, 'Цвет', 'Синий'),
  (5, 'Размеры', '210x95x90 см');

CREATE VIEW `v_order_items_products` AS
SELECT
  oi.id AS order_item_id,
  oi.order_id,
  o.date AS order_date,
  o.user_id,
  oi.product_id,
  p.name AS product_name,
  p.category_id,
  c.name AS category_name,
  oi.quantity,
  oi.price AS unit_price,
  oi.quantity * oi.price AS total_price,
  o.readiness
FROM `order_items` oi
INNER JOIN `orders` o ON o.id = oi.order_id
INNER JOIN `products` p ON p.id = oi.product_id
LEFT JOIN `categories` c ON c.id = p.category_id;

CREATE VIEW `v_products_characteristics` AS
SELECT
  p.id AS product_id,
  p.name AS product_name,
  p.category_id,
  c.name AS category_name,
  p.quantity,
  p.price,
  p.photo,
  pc.name AS characteristic_name,
  pc.value AS characteristic_value
FROM `products` p
LEFT JOIN `categories` c ON c.id = p.category_id
LEFT JOIN `product_characteristics` pc ON pc.product_id = p.id;

CREATE VIEW `v_order_details` AS
SELECT
  o.id AS order_id,
  o.date AS order_date,
  o.user_id,
  u.login AS user_login,
  u.name AS user_name,
  oi.id AS order_item_id,
  p.id AS product_id,
  p.name AS product_name,
  c.name AS category_name,
  oi.quantity,
  oi.price AS unit_price,
  oi.quantity * oi.price AS total_price,
  GROUP_CONCAT(CONCAT(pc.name, ': ', pc.value) ORDER BY pc.name SEPARATOR ', ') AS product_characteristics,
  o.readiness
FROM `orders` o
INNER JOIN `users` u ON u.id = o.user_id
INNER JOIN `order_items` oi ON oi.order_id = o.id
INNER JOIN `products` p ON p.id = oi.product_id
LEFT JOIN `categories` c ON c.id = p.category_id
LEFT JOIN `product_characteristics` pc ON pc.product_id = p.id
GROUP BY
  o.id, o.date, o.user_id, u.login, u.name,
  oi.id, p.id, p.name, c.name, oi.quantity, oi.price, o.readiness;

CREATE TABLE IF NOT EXISTS `employees` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `position` varchar(100) NOT NULL,
  `phone` varchar(25) NOT NULL,
  `email` varchar(50) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_general_ci;
