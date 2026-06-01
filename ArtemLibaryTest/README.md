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

### 4) Замените встроенные окна входа/регистрации своими

Если нужно оставить готовую авторизацию, сессию и главное меню библиотеки, но нарисовать свои WPF-окна в проекте-потребителе, задайте фабрики в `AuthUiOptions`:

```csharp
var authService = new MySqlAuthService(DbConfig.ConnectionString);
var options = new AuthUiOptions
{
    AppTitle = "Exam Demo",
    MainWelcomeText = "Готовое главное меню из библиотеки",
    LoginWindowFactory = context => new MyLoginWindow(context),
    RegisterWindowFactory = context => new MyRegisterWindow(context)
};

AuthUiLauncher.CreateLoginWindow(authService, options).Show();
```

В своем окне входа сохраните `AuthUiContext`, а в обработчике кнопки передайте `TextBox` логина и `PasswordBox`/`TextBox` пароля. Метод сам возьмет текст из контролов, выполнит проверку через `IAuthService.Login`, заполнит `Session.CurrentUser` и откроет встроенное главное меню:

```csharp
public partial class MyLoginWindow : Window
{
    private readonly AuthUiContext _context;

    public MyLoginWindow(AuthUiContext context)
    {
        InitializeComponent();
        _context = context;
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        AuthUiLauncher.TryLoginAndOpenMain(_context, LoginBox, PasswordBox, this);
    }

    private void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        AuthUiLauncher.OpenRegisterWindow(_context, this);
    }
}
```

В своем окне регистрации используйте аналогичный helper. Он берет данные из `TextBox`/`PasswordBox`, вызывает `IAuthService.Register`, показывает стандартные сообщения и после успешной регистрации открывает окно входа — встроенное или ваше, если задан `LoginWindowFactory`:

```csharp
private void RegisterButton_Click(object sender, RoutedEventArgs e)
{
    AuthUiLauncher.TryRegisterAndOpenLogin(
        _context,
        LoginBox,
        PasswordBox,
        NameBox,
        PhoneBox,
        EmailBox,
        this);
}
```

Также доступны перегрузки, куда можно передать `IAuthService` и `AuthUiOptions` напрямую, если не хотите хранить `AuthUiContext` в окне.


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

