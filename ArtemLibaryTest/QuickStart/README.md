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
