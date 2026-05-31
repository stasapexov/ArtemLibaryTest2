﻿# ArtemLibaryTest

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

Если `ComboBox` используется как фильтр товаров, сначала загрузите категории, а потом товары.
Для верхней границы цены используйте `AddWhereMax`, не второй `AddWhereMin`:

```csharp
private void LoadCombo()
{
    _db.LoadCategoriesToComboBox(ComboData);
}

private void LoadData(double? min = null, double? max = null, string name = "")
{
    var sql = new StringBuilder(@"
SELECT p.id, p.name, c.name AS category_name, p.quantity, p.price, p.photo
FROM products p
LEFT JOIN categories c ON c.id = p.category_id
WHERE 1=1");

    var parameters = new List<MySqlParameter>();
    DbHelper.AddWhereMin(sql, parameters, "p.price", "@min", min);
    DbHelper.AddWhereMax(sql, parameters, "p.price", "@max", max);
    DbHelper.AddWhereLikeAnyWord(sql, parameters, "p.name", "@name", name);
    DbHelper.AddWhereEqualsFromComboBox(sql, parameters, "p.category_id", "@category", ComboData);

    ItemsData.ItemsSource = _db.GetTableWithImagePath(sql.ToString(), parameters.ToArray()).DefaultView;
}
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