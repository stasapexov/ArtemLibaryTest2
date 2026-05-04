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
