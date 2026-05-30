# ArtemLibaryTest

`ArtemLibaryTest` — это WPF/.NET библиотека для быстрого запуска типового desktop‑приложения с авторизацией, ролями пользователей, готовыми UI-компонентами и интеграцией с MySQL.

## Описание проекта

Библиотека помогает быстро собрать каркас учебного/демо-проекта:

- авторизация и сессия пользователя;
- готовые модели и шаблоны для меню/навигации;
- вспомогательные методы для SQL-фильтрации и работы с `DataTable`;
- удобные методы для вывода изображений из пути файла и из BLOB-полей MySQL;
- quick start-компоненты для окна входа и пользовательских страниц.

Это снижает количество шаблонного кода в WPF-проектах и ускоряет запуск MVP/демо.

## Установка

```bash
dotnet add package ArtemLibaryTest
```

## Быстрый старт

### 1) Подключите namespace

```csharp
using ArtemLibaryTest.Core;
using ArtemLibaryTest.Models;
using ArtemLibaryTest.QuickStart;
```

### 2) Настройте подключение к БД

```csharp
public static class DbConfig
{
    public const string ConnectionString =
        "Server=127.0.0.1;Port=3306;Database=exam_demo;Uid=root;Pwd=;SslMode=None;Protocol=Tcp;";
}
```

### 3) Запустите готовое окно авторизации

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    var authService = new MySqlAuthService(DbConfig.ConnectionString);
    var options = new AuthUiOptions
    {
        AppTitle = "Exam Demo",
        MainWelcomeText = "Готовое главное меню из библиотеки",
        IsSettingsVisible = true,
        MenuProvider = new AppMenuProvider()
    };

    var loginWindow = AuthUiLauncher.CreateLoginWindow(authService, options);
    loginWindow.Show();
}
```

## Примеры использования

### Пример 1: Фильтрация SQL через `DbHelper`

```csharp
var db = new DbHelper(DbConfig.ConnectionString);
var sql = new StringBuilder(@"
SELECT p.id, p.name, p.price, p.photo
FROM products p
WHERE 1=1");

var parameters = new List<MySqlParameter>();
DbHelper.AddWhereMin(sql, parameters, "p.price", "@min", 100);
DbHelper.AddWhereLikeAnyWord(sql, parameters, "p.name", "@name", "цемент м500");

var table = db.GetTableWithImagePath(sql.ToString(), parameters.ToArray());
ProductsGrid.ItemsSource = table.DefaultView;
```

### Пример 2: Загрузка категорий в `ComboBox`

```csharp
var db = new DbHelper(DbConfig.ConnectionString);
db.LoadCategoriesToComboBox(CategoryComboBox);
```

### Пример 3: Вывод BLOB-аватарок из MySQL

```csharp
var db = new DbHelper(DbConfig.ConnectionString);

UsersGrid.ItemsSource = db.GetTableWithBlobImage(@"
    SELECT id, name, login, phone, email, status, money, img
    FROM users
    ORDER BY id;").DefaultView;
```

Для `DataGrid`:

```xml
<DataGridTemplateColumn Header="Аватар">
    <DataGridTemplateColumn.CellTemplate>
        <DataTemplate>
            <Image Source="{Binding ImgSource}" Width="48" Height="48" Stretch="UniformToFill" />
        </DataTemplate>
    </DataGridTemplateColumn.CellTemplate>
</DataGridTemplateColumn>
```


### Пример 4: Универсальная загрузка `ComboBox`

Метод подходит для категорий, пунктов выдачи, сотрудников и любых справочников. SQL остается обычным и совместимым со старым MySQL.

```csharp
var db = new DbHelper(DbConfig.ConnectionString);

db.LoadComboBox(
    CategoryComboBox,
    "SELECT id, name FROM categories ORDER BY name",
    displayColumn: "name",
    valueColumn: "id",
    firstItemText: "Все категории");
```

### Пример 5: Карточки характеристик из своего `SELECT`

Сначала пишете обычный SQL-запрос с теми колонками, которые нужно показать:

```csharp
var db = new DbHelper(DbConfig.ConnectionString);

var products = db.LoadCardTable(@"
SELECT id, article, name, material, color, dimensions, price
FROM products
ORDER BY name;");
```

Потом передаете результат в WPF-контейнер, например в `StackPanel` с именем `ProductsPanel`:

```csharp
db.AddCardsFromTable(
    ProductsPanel,
    products,
    "name",
    "article",
    "material",
    "color",
    "dimensions",
    "price");
```

В итоге каждая строка из запроса становится карточкой: заголовок берется из `name`, а характеристики — из перечисленных колонок.

### SQL для демо-БД

`Sql.demo_reset.sql` встраивается в сборку как демо-ресурс и может быть явно запущен через `new MySqlAuthService(connectionString).ResetDemoDatabase();`. Автоматического скрытого запуска из окна входа нет.
