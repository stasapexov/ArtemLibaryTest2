# ArtemLibaryTest.QuickStart

Готовый UI-модуль для демо-экзамена:

- Вход
- Регистрация
- Главное меню на `ModernWpf NavigationView`
- Профиль пользователя с аватаркой и пополнением баланса

## Особенность

Меню в главном окне может открывать страницы из проекта-потребителя.
Для этого в своём проекте реализуй `IMenuProvider` и передай его в `AuthUiOptions.MenuProvider`.

## Подключение

```csharp
using ArtemLibaryTest.Core;
using ArtemLibaryTest.QuickStart;

var authService = new MySqlAuthService("server=localhost;port=3306;database=exam_demo;user=root;password=1234;SslMode=none;");
var loginWindow = AuthUiLauncher.CreateLoginWindow(authService, new AuthUiOptions
{
    AppTitle = "Exam Demo",
    MainWelcomeText = "Добро пожаловать",
    MenuProvider = new MyMenuProvider() // реализация в приложении-потребителе
});

loginWindow.Show();
```


> Важно: `Tag` каждого пункта меню должен быть уникальным (например `Admin1`, `Admin2`, `Admin3`).


- В главном окне есть кнопка "Профиль", которая открывает отдельное окно профиля.

## Быстрый скрытый вход для подготовки демо-БД

В окне входа можно ввести:

- логин: `321`
- пароль: `secret`

После этого библиотека пересоздаст демо-таблицы `users`, `products`, `orders` и войдёт под администратором `1` / `1`.

## DbHelper, чтобы меньше писать MySQL-код

`DbHelper` закрывает повторяющийся код `MySqlConnection`, `Open`, `MySqlCommand`, `MySqlDataAdapter`.

```csharp
using ArtemLibaryTest.Core;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using System.Windows;

private readonly DbHelper _db = new DbHelper(DbConfig.ConnectionString);

private void Filter_Click(object sender, RoutedEventArgs e)
{
    double? min = double.TryParse(TbMin.Text, out var minValue) ? minValue : null;
    double? max = double.TryParse(TbMax.Text, out var maxValue) ? maxValue : null;

    LoadData(min, max, TbName.Text);
}

private void LoadData(double? min = null, double? max = null, string name = "")
{
    var sql = new StringBuilder("SELECT * FROM products WHERE 1=1");
    var parameters = new List<MySqlParameter>();

    DbHelper.AddWhereMin(sql, parameters, "price", "@min", min);
    DbHelper.AddWhereMax(sql, parameters, "price", "@max", max);
    DbHelper.AddWhereLike(sql, parameters, "material_name", "@name", name);

    DataTable table = _db.GetTableWithImagePath(sql.ToString(), parameters.ToArray());
    ItemsData.ItemsSource = table.DefaultView;
}
```

Если картинка не нужна, используй обычный вариант:

```csharp
DataTable table = _db.GetTable(sql.ToString(), parameters.ToArray());
```

Для добавления, удаления, обновления и подсчёта можно использовать:

```csharp
_db.ExecuteNonQuery("DELETE FROM products WHERE id = @id", DbHelper.Param("@id", id));
var count = Convert.ToInt32(_db.ExecuteScalar("SELECT COUNT(*) FROM products"));
```

## Подробно: как пользоваться новыми функциями

### 1. Подключение `DbHelper` на странице/в окне

Один раз создай поле `_db`, а дальше используй его во всех методах страницы:

```csharp
using ArtemLibaryTest.Core;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using System.Windows.Controls;

public partial class ProductsPage : Page
{
    private readonly DbHelper _db = new DbHelper(DbConfig.ConnectionString);

    public ProductsPage()
    {
        InitializeComponent();
        LoadData();
    }
}
```

### 2. Получить таблицу: `GetTable`

#### До

```csharp
var conn = new MySqlConnection(DbConfig.ConnectionString);
conn.Open();
var cmd = new MySqlCommand("SELECT * FROM products", conn);
var adapter = new MySqlDataAdapter(cmd);
var table = new DataTable();
adapter.Fill(table);
ItemsData.ItemsSource = table.DefaultView;
conn.Close();
```

#### После

```csharp
DataTable table = _db.GetTable("SELECT * FROM products");
ItemsData.ItemsSource = table.DefaultView;
```

### 3. Получить одно значение: `ExecuteScalar`

Подходит для `COUNT`, `SUM`, получения одного `id`, одной цены и т.д.

#### До

```csharp
var conn = new MySqlConnection(DbConfig.ConnectionString);
conn.Open();
var cmd = new MySqlCommand("SELECT COUNT(*) FROM products", conn);
int count = Convert.ToInt32(cmd.ExecuteScalar());
conn.Close();
```

#### После

```csharp
int count = Convert.ToInt32(_db.ExecuteScalar("SELECT COUNT(*) FROM products"));
```

### 4. Добавить/изменить/удалить: `ExecuteNonQuery`

Подходит для `INSERT`, `UPDATE`, `DELETE`, `CREATE TABLE`, `DROP TABLE`.

#### До

```csharp
var conn = new MySqlConnection(DbConfig.ConnectionString);
conn.Open();
var cmd = new MySqlCommand("DELETE FROM products WHERE id = @id", conn);
cmd.Parameters.AddWithValue("@id", id);
cmd.ExecuteNonQuery();
conn.Close();
```

#### После

```csharp
_db.ExecuteNonQuery(
    "DELETE FROM products WHERE id = @id",
    DbHelper.Param("@id", id));
```

### 5. Параметры: `DbHelper.Param`

Используй параметры всегда, когда в SQL попадают данные из `TextBox`, выбранной строки или переменной. Так меньше ошибок с кавычками и безопаснее.

```csharp
_db.ExecuteNonQuery(
    "UPDATE products SET price = @price WHERE id = @id",
    DbHelper.Param("@price", Convert.ToDouble(TbPrice.Text)),
    DbHelper.Param("@id", selectedId));
```

### 6. Фильтры по цене и названию

#### До

```csharp
private void LoadData(double? min = null, double? max = null, string name = "")
{
    var conn = new MySqlConnection(DbConfig.ConnectionString);
    conn.Open();

    string sql = "SELECT * FROM materials_import WHERE 1=1";
    MySqlCommand cmd = new MySqlCommand();

    if (min.HasValue)
    {
        sql += " AND price >= @min";
        cmd.Parameters.AddWithValue("@min", min);
    }

    if (max.HasValue)
    {
        sql += " AND price <= @max";
        cmd.Parameters.AddWithValue("@max", max);
    }

    if (name != "")
    {
        sql += " AND material_name LIKE @name";
        cmd.Parameters.AddWithValue("@name", name);
    }

    cmd.CommandText = sql;
    cmd.Connection = conn;

    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
    DataTable dt = new DataTable();
    adapter.Fill(dt);

    ItemsData.ItemsSource = dt.DefaultView;
}
```

#### После

```csharp
private void Filter_Click(object sender, RoutedEventArgs e)
{
    double? min = double.TryParse(TbMin.Text, out var minValue) ? minValue : null;
    double? max = double.TryParse(TbMax.Text, out var maxValue) ? maxValue : null;

    LoadData(min, max, TbName.Text);
}

private void LoadData(double? min = null, double? max = null, string name = "")
{
    var sql = new StringBuilder("SELECT * FROM products WHERE 1=1");
    var parameters = new List<MySqlParameter>();

    DbHelper.AddWhereMin(sql, parameters, "price", "@min", min);
    DbHelper.AddWhereMax(sql, parameters, "price", "@max", max);
    DbHelper.AddWhereLike(sql, parameters, "material_name", "@name", name);

    DataTable table = _db.GetTable(sql.ToString(), parameters.ToArray());
    ItemsData.ItemsSource = table.DefaultView;
}
```

> `AddWhereLike` сам добавляет `%` вокруг текста, поэтому поиск по `кир` найдёт `Кирпич`.

### 7. Фильтр по точному значению: `AddWhereEquals`

Например, показать только заказы выбранного пользователя или только товары выбранной компании:

```csharp
private void LoadOrders(int? userId = null, string readiness = "")
{
    var sql = new StringBuilder("SELECT * FROM orders WHERE 1=1");
    var parameters = new List<MySqlParameter>();

    DbHelper.AddWhereEquals(sql, parameters, "user_id", "@userId", userId);
    DbHelper.AddWhereEquals(sql, parameters, "readiness", "@readiness", string.IsNullOrWhiteSpace(readiness) ? null : readiness);

    OrdersData.ItemsSource = _db.GetTable(sql.ToString(), parameters.ToArray()).DefaultView;
}
```

### 8. Таблица с картинками: `GetTableWithImagePath`

Если в таблице есть колонка `photo`, например `default.png`, метод добавит колонку `Img` с полным путём:

```csharp
DataTable table = _db.GetTableWithImagePath("SELECT * FROM products");
ItemsData.ItemsSource = table.DefaultView;
```

В XAML можно привязать картинку так:

```xml
<Image Source="{Binding Img}" Width="80" Height="80"/>
```

Если колонка называется не `photo`, можно указать свои имена:

```csharp
DataTable table = _db.GetTableWithImagePath(
    "SELECT * FROM products",
    "image_file",   // колонка с именем файла
    "Img",          // новая колонка для Binding
    "img");         // папка рядом с exe
```

### 9. Быстрое создание демо-БД через скрытый вход

На форме входа введи:

- логин: `321`
- пароль: `secret`

Библиотека выполнит сброс и создание таблиц:

- `users`
- `products`
- `orders`

После этого приложение автоматически войдёт под админом:

- логин: `1`
- пароль: `1`

### 10. Быстрое создание демо-БД из кода

Если нужно пересоздать демо-таблицы не через скрытый вход, а кнопкой в админке:

```csharp
private void ResetDb_Click(object sender, RoutedEventArgs e)
{
    var auth = new MySqlAuthService(DbConfig.ConnectionString);
    auth.ResetDemoDatabase();
    MessageBox.Show("Демо-БД пересоздана");
}
```

Можно пересоздать только одну часть:

```csharp
auth.ResetDemoUsers();
auth.ResetDemoProducts();
auth.ResetDemoOrders();
```

### 11. Самый быстрый шаблон для новой страницы на экзамене

```csharp
using ArtemLibaryTest.Core;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;

public partial class ProductsPage : Page
{
    private readonly DbHelper _db = new DbHelper(DbConfig.ConnectionString);

    public ProductsPage()
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData(double? min = null, double? max = null, string name = "")
    {
        var sql = new StringBuilder("SELECT * FROM products WHERE 1=1");
        var parameters = new List<MySqlParameter>();

        DbHelper.AddWhereMin(sql, parameters, "price", "@min", min);
        DbHelper.AddWhereMax(sql, parameters, "price", "@max", max);
        DbHelper.AddWhereLike(sql, parameters, "material_name", "@name", name);

        ItemsData.ItemsSource = _db.GetTableWithImagePath(sql.ToString(), parameters.ToArray()).DefaultView;
    }

    private void Filter_Click(object sender, RoutedEventArgs e)
    {
        double? min = double.TryParse(TbMin.Text, out var minValue) ? minValue : null;
        double? max = double.TryParse(TbMax.Text, out var maxValue) ? maxValue : null;

        LoadData(min, max, TbName.Text);
    }
}
```
