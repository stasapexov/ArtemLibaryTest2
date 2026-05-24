# SQL syntax fixes for `Testxaml`

The MySQL syntax error comes from this malformed projection:

```sql
p.name AS material_name AS category
```

A column can only have one alias. Use separate expressions for material name and category name, for example:

```sql
SELECT
  p.id,
  p.name AS material_name,
  c.name AS category,
  p.quantity,
  p.price,
  p.photo AS Img
FROM products p
LEFT JOIN categories c ON c.id = p.category_id
WHERE 1=1
```

> Avoid ending dynamic SQL templates with `;` when you append filters later.

## Fix for `@currentOrderId must be defined`

When using `MySqlCommand` with parameters, `@currentOrderId` is parsed as a command parameter (not a SQL user variable) unless the connection string explicitly enables user variables. That is why you get:

`Parameter '@currentOrderId' must be defined`.

### Safer fix (recommended): do not use SQL user variable

Use `LAST_INSERT_ID()` directly in the second insert:

```sql
INSERT INTO orders (date, user_id, total_price)
VALUES (NOW(), @user_id, @totalPrice);

INSERT INTO order_items (order_id, product_id, quantity, unit_price)
VALUES (LAST_INSERT_ID(), @product_id, @count, @price);

UPDATE products
SET quantity = quantity - @count
WHERE id = @product_id;

UPDATE users
SET money = money - @totalPrice
WHERE id = @user_id;
```

### Alternative

If you intentionally use SQL user variables (`@currentOrderId`), add `Allow User Variables=true` to the MySQL connection string.

And in `AddWhereLikeAnyWord`, filter by actual table column (`p.name`) instead of alias (`material_name`) in `WHERE`.
