# SampleWpf: пример замены встроенных окон входа и регистрации

UI-экраны входа, регистрации и главного меню находятся в библиотеке `ArtemLibaryTest.QuickStart`.
Этот пример показывает, как заменить только окна входа и регистрации на свои WPF-окна, но оставить:

- проверку пользователя через `MySqlAuthService`;
- заполнение `Session.CurrentUser`;
- переход во встроенное главное меню библиотеки после успешного входа;
- переходы между своим окном входа и своим окном регистрации.

## SQL для phpMyAdmin / MySQL

```sql
CREATE DATABASE IF NOT EXISTS exam_demo;
USE exam_demo;

CREATE TABLE IF NOT EXISTS users (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    password VARCHAR(100) NOT NULL,
    login VARCHAR(100) NOT NULL UNIQUE,
    phone VARCHAR(20) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'user'
);
```

## 1. Отключите автоматический запуск стандартного `MainWindow`

В `App.xaml` не должен быть указан `StartupUri="MainWindow.xaml"`, потому что стартовое окно создает код в `App.xaml.cs`:

```xml
<Application x:Class="SampleWpf.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:SampleWpf">
    <Application.Resources />
</Application>
```

## 2. Подключите свои окна через `AuthUiOptions`

В `App.xaml.cs` создайте `MySqlAuthService`, настройте `AuthUiOptions` и укажите фабрики своих окон:

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    var authService = new MySqlAuthService(DbConfig.ConnectionString);
    var options = new AuthUiOptions
    {
        AppTitle = "Exam Demo",
        MainWelcomeText = "Готовое главное меню из библиотеки",
        LoginWindowFactory = context => new CustomLoginWindow(context),
        RegisterWindowFactory = context => new CustomRegisterWindow(context)
    };

    var loginWindow = AuthUiLauncher.CreateLoginWindow(authService, options);
    loginWindow.Show();
}
```

## 3. Свое окно входа

`CustomLoginWindow` хранит `AuthUiContext`. В обработчике кнопки входа передайте в библиотечный helper `TextBox` логина и `PasswordBox` пароля:

```csharp
private void Login_Click(object sender, RoutedEventArgs e)
{
    AuthUiLauncher.TryLoginAndOpenMain(_context, LoginBox, PasswordBox, this);
}
```

Что делает `TryLoginAndOpenMain`:

1. берет `LoginBox.Text.Trim()` и `PasswordBox.Password`;
2. вызывает `IAuthService.Login(login, password)`;
3. если пользователь найден — записывает его в `Session.CurrentUser`;
4. открывает встроенное главное меню библиотеки;
5. закрывает текущее окно.

Переход на регистрацию из своего окна входа:

```csharp
private void OpenRegister_Click(object sender, RoutedEventArgs e)
{
    AuthUiLauncher.OpenRegisterWindow(_context, this);
}
```

Если в `AuthUiOptions.RegisterWindowFactory` задано свое окно, откроется оно. Если фабрика не задана, библиотека откроет встроенное окно регистрации.

## 4. Свое окно регистрации

`CustomRegisterWindow` так же хранит `AuthUiContext`. В обработчике кнопки регистрации передайте контролы в `TryRegisterAndOpenLogin`:

```csharp
private void Register_Click(object sender, RoutedEventArgs e)
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

Что делает `TryRegisterAndOpenLogin`:

1. берет текст из `LoginBox`, `PasswordBox`, `NameBox`, `PhoneBox`, `EmailBox`;
2. вызывает `IAuthService.Register(...)`;
3. показывает стандартное сообщение об успехе или о занятом логине;
4. после успешной регистрации открывает окно входа — свое или встроенное, в зависимости от `LoginWindowFactory`.

Переход назад на вход:

```csharp
private void OpenLogin_Click(object sender, RoutedEventArgs e)
{
    AuthUiLauncher.OpenLoginWindow(_context, this);
}
```

## Что ты получаешь сразу

- Свой дизайн окна входа.
- Свой дизайн окна регистрации.
- Готовую проверку логина/пароля через библиотеку.
- Автоматическое заполнение `Session.CurrentUser`.
- Готовое главное меню библиотеки после успешного входа.
