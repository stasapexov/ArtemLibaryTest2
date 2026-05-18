1.Добавьте в корень проекта AppMenuProvider.cs c кодом:
using ArtemLibaryTest.Models;
using ArtemLibaryTest.QuickStart;

public class AppMenuProvider : IMenuProvider
    {
        public IEnumerable<NavMenuItem> GetMenuItems(Users currentUser)
        {
            return
            [
                new NavMenuItem
                {
                    Title = "Главная",
                    Tag = "Home",
                    Icon = "Home",
                    Roles = ["admin", "user", "manager"],
                    CreatePage = () => new Pages.HamePage()
                },
                new NavMenuItem
                {
                    Title = "Профиль",
                    Tag = "Profile",
                    Icon = "Contact",
                    Roles = ["admin", "user", "manager"],
                    CreatePage = () => new Pages.ProfilePage()
                },
                new NavMenuItem
                {
                    Title = "Админ панель",
                    Tag = "Admin",
                    Icon = "Admin",
                    Roles = ["admin"],
                    CreatePage = () => new Pages.AdminPage()
                }
            ];
        }
    }


2.Добавьте DbConfig.cs:

public static class DbConfig
    {
        // Подставь свои значения из phpMyAdmin/MySQL
        public const string ConnectionString = "Server=127.0.0.1;Port=3306;Database=stroy;Uid=root;Pwd=;Protocol=Tcp;AllowZeroDateTime=true";
    }


3.В App.xaml.cs: 

using ArtemLibaryTest.Core;
using ArtemLibaryTest.QuickStart;

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

4.DbConfig:
Server=127.0.0.1;Port=3306;Database=exam_demo;Uid=root;Pwd=;SslMode=None;Protocol=Tcp;
Вариант Б (если MySQL 8 ругается на auth key):
Server=127.0.0.1;Port=3306;Database=exam_demo;Uid=root;Pwd=;SslMode=None;Protocol=Tcp;AllowPublicKeyRetrieval=True;
## Вывод аватарок пользователей из BLOB

Для админской страницы можно получить таблицу пользователей сразу с готовой колонкой для `Image.Source`:

```csharp
using ArtemLibaryTest.Core;

var db = new DbHelper(DbConfig.ConnectionString);

UsersGrid.ItemsSource = db.GetTableWithBlobImage(@"
    SELECT id, name, login, phone, email, status, money, img
    FROM users
    ORDER BY id;").DefaultView;
```

Пример колонки в `DataGrid`:

```xml
<DataGridTemplateColumn Header="Аватар">
    <DataGridTemplateColumn.CellTemplate>
        <DataTemplate>
            <Image Source="{Binding ImgSource}" Width="48" Height="48" Stretch="UniformToFill" />
        </DataTemplate>
    </DataGridTemplateColumn.CellTemplate>
</DataGridTemplateColumn>
```

По умолчанию метод берет BLOB из колонки `img` и добавляет новую колонку `ImgSource` типа `BitmapImage`. Если в вашей таблице другие названия колонок, используйте перегрузку:

```csharp
UsersGrid.ItemsSource = db.GetTableWithBlobImage(
    "SELECT id, name, avatar_blob FROM users",
    "avatar_blob",
    "AvatarImage").DefaultView;
```