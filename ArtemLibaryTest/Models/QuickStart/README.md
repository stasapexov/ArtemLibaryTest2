# ArtemLibaryTest.QuickStart

Готовый UI-модуль для демо-экзамена:

- Вход
- Регистрация
- Главное меню (с `ID пользователя`)

## Подключение

```csharp
using ArtemLibaryTest.Core;
using ArtemLibaryTest.QuickStart;

var authService = new MySqlAuthService("server=localhost;port=3306;database=exam_demo;user=root;password=1234;SslMode=none;");
var loginWindow = AuthUiLauncher.CreateLoginWindow(authService, new AuthUiOptions
{
    AppTitle = "Exam Demo",
    MainWelcomeText = "Добро пожаловать"
});

loginWindow.Show();
```