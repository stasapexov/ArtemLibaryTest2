Теперь UI-экраны лежат в самой библиотеке `ArtemLibaryTest.QuickStart`.
В приложении нужно только передать `IAuthService` и вызвать `AuthUiLauncher.CreateLoginWindow(...)`.

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

## Что нужно сделать в приложении

1. Прописать строку подключения в `DbConfig.cs`.
Comment view2. В `App.xaml.cs` создать `MySqlAuthService`.
3. Вызвать `AuthUiLauncher.CreateLoginWindow(authService, options)`.

## Что ты получаешь сразу

- Готовую страницу входа.
- Готовую страницу регистрации.
- Готовое главное окно с `ID пользователя` в label.