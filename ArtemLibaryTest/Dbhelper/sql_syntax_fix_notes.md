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
WHERE 1=1;
```

Also fix the multi-statement order SQL:

- add missing semicolons
- `LAST_INSERT_ID()` should be used directly or assigned with `SET`

Correct version:

```sql
INSERT INTO orders (date, user_id, total_price)
VALUES (NOW(), @user_id, @totalPrice);

SET @currentOrderId = LAST_INSERT_ID();

INSERT INTO order_items (order_id, product_id, quantity, unit_price)
VALUES (@currentOrderId, @product_id, @count, @price);

UPDATE products
SET quantity = quantity - @count
WHERE id = @product_id;

UPDATE users
SET money = money - @totalPrice
WHERE id = @user_id;
```

And in `AddWhereLikeAnyWord`, filter by actual table column (`p.name`) instead of alias (`material_name`) in `WHERE`.
