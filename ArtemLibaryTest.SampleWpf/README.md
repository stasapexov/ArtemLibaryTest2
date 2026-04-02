# Максимально быстрый старт: готовый вход, регистрация и главное окно из библиотеки

Теперь UI-экраны входа/регистрации и главное меню лежат в библиотеке `ArtemLibaryTest.QuickStart`.
Главное меню построено на `ModernWpf NavigationView` и может открывать страницы из проекта-потребителя.

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
2. В `App.xaml.cs` создать `MySqlAuthService`.
3. Создать `IMenuProvider` в своём проекте и вернуть нужные страницы по ролям.
4. Вызвать `AuthUiLauncher.CreateLoginWindow(authService, options)`.

## Что ты получаешь сразу

- Готовую страницу входа.
- Готовую страницу регистрации.
- Готовое главное окно с `NavigationView`.
- Ролевое меню (admin/user/manager) для страниц из твоего проекта.
