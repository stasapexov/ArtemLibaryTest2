Подробная инструкция по DbHelper
================================

DbHelper — это небольшой помощник для работы с MySQL из WPF/WinForms/консольного
приложения на .NET. Класс находится в пространстве имен ArtemLibaryTest.Core и
использует пакет MySql.Data.

1. Подключение библиотеки
------------------------

Добавьте using в файл, где будете работать с базой данных:

using ArtemLibaryTest.Core;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;

2. Строка подключения
---------------------

Создайте в своем проекте файл DbConfig.cs и храните строку подключения в одном
месте:

public static class DbConfig
{
    public const string ConnectionString =
        "Server=127.0.0.1;Port=3306;Database=exam_demo;Uid=root;Pwd=;SslMode=None;Protocol=Tcp;";
}

Если MySQL 8 просит публичный ключ, используйте вариант с AllowPublicKeyRetrieval:

public static class DbConfig
{
    public const string ConnectionString =
        "Server=127.0.0.1;Port=3306;Database=exam_demo;Uid=root;Pwd=;SslMode=None;Protocol=Tcp;AllowPublicKeyRetrieval=True;";
}

3. Создание DbHelper
--------------------

Создайте экземпляр класса и передайте строку подключения:

var db = new DbHelper(DbConfig.ConnectionString);

Обычно DbHelper создают в окне, странице, сервисе или репозитории, где выполняются
SQL-запросы.

4. Параметры запросов: DbHelper.Param
-------------------------------------

Всегда передавайте значения через параметры, а не склеивайте SQL-строку вручную.
Это защищает от SQL-инъекций и проблем с кавычками.

var userId = 5;
var table = db.GetTable(
    "SELECT * FROM users WHERE id = @id",
    DbHelper.Param("@id", userId));

Если значение null, DbHelper.Param автоматически передаст DBNull.Value.

5. Получение таблицы: GetTable
------------------------------

GetTable выполняет SELECT и возвращает DataTable.

DataTable users = db.GetTable(
    "SELECT id, login, role FROM users WHERE role = @role",
    DbHelper.Param("@role", "admin"));

Пример привязки к DataGrid в WPF:

UsersGrid.ItemsSource = users.DefaultView;

6. Получение одного значения: ExecuteScalar
-------------------------------------------

ExecuteScalar удобно использовать для COUNT, MAX, SUM, получения одного id или
одного поля.

object? result = db.ExecuteScalar(
    "SELECT COUNT(*) FROM users WHERE role = @role",
    DbHelper.Param("@role", "user"));

int count = Convert.ToInt32(result);

7. Выполнение INSERT, UPDATE, DELETE: ExecuteNonQuery
-----------------------------------------------------

ExecuteNonQuery возвращает количество измененных строк.

int added = db.ExecuteNonQuery(
    "INSERT INTO users (login, password, role) VALUES (@login, @password, @role)",
    DbHelper.Param("@login", "ivan"),
    DbHelper.Param("@password", "12345"),
    DbHelper.Param("@role", "user"));

int updated = db.ExecuteNonQuery(
    "UPDATE users SET role = @role WHERE id = @id",
    DbHelper.Param("@role", "admin"),
    DbHelper.Param("@id", 5));

int deleted = db.ExecuteNonQuery(
    "DELETE FROM users WHERE id = @id",
    DbHelper.Param("@id", 5));

8. Динамические фильтры для поиска
----------------------------------

Методы AddWhereEquals, AddWhereMin, AddWhereMax и AddWhereLike помогают собрать
WHERE-блок только из заполненных фильтров.

string? searchText = SearchTextBox.Text;
double? minPrice = 100;
double? maxPrice = 500;

var sql = new StringBuilder("SELECT * FROM products WHERE 1=1");
var parameters = new List<MySqlParameter>();

DbHelper.AddWhereLike(sql, parameters, "name", "@name", searchText);
DbHelper.AddWhereMin(sql, parameters, "price", "@minPrice", minPrice);
DbHelper.AddWhereMax(sql, parameters, "price", "@maxPrice", maxPrice);

DataTable products = db.GetTable(sql.ToString(), parameters.ToArray());

Важно: названия колонок в этих методах подставляются в SQL напрямую. Передавайте
только заранее известные имена колонок из вашего кода, а не текст от пользователя.

9. Работа с изображениями: GetTableWithImagePath
------------------------------------------------

Если в таблице есть колонка с именем файла изображения, DbHelper может добавить
дополнительную колонку с полным путем к файлу.

По умолчанию метод ожидает:
- колонку photo с именем файла;
- папку img рядом с exe-файлом приложения;
- новую колонку Img, в которую будет записан полный путь.

DataTable products = db.GetTableWithImagePath(
    "SELECT id, name, photo FROM products");

ProductsGrid.ItemsSource = products.DefaultView;

Если имя файла пустое, будет использован default.png.

10. Свои имена колонок и папки изображений
------------------------------------------

Если в базе данных другие имена колонок, используйте перегрузку метода:

DataTable products = db.GetTableWithImagePath(
    "SELECT id, title, image_file FROM products",
    photoColumn: "image_file",
    imagePathColumn: "ImagePath",
    imageFolder: "Images");

В этом примере DbHelper ищет имя файла в колонке image_file, добавляет колонку
ImagePath и строит путь относительно папки Images рядом с exe-файлом.

11. AddImagePathColumn отдельно
-------------------------------

Если DataTable уже получен другим способом, можно добавить колонку с путями
отдельно:

DataTable table = db.GetTable("SELECT id, name, photo FROM products");
DbHelper.AddImagePathColumn(table, "photo", "Img", "img");

12. Полный пример загрузки товаров в WPF
----------------------------------------

private readonly DbHelper _db = new DbHelper(DbConfig.ConnectionString);

private void LoadProducts()
{
    var sql = new StringBuilder("SELECT id, name, price, photo FROM products WHERE 1=1");
    var parameters = new List<MySqlParameter>();

    DbHelper.AddWhereLike(sql, parameters, "name", "@name", SearchTextBox.Text);

    if (double.TryParse(MinPriceTextBox.Text, out var minPrice))
    {
        DbHelper.AddWhereMin(sql, parameters, "price", "@minPrice", minPrice);
    }

    DataTable table = _db.GetTableWithImagePath(
        sql.ToString(),
        photoColumn: "photo",
        imagePathColumn: "Img",
        imageFolder: "img",
        parameters.ToArray());

    ProductsGrid.ItemsSource = table.DefaultView;
}

13. Рекомендации
----------------

- Всегда используйте параметры через DbHelper.Param.
- Проверяйте, что MySQL запущен и база данных существует.
- Проверяйте правильность имени базы, пользователя, пароля и порта в строке подключения.
- Не передавайте пользовательский ввод как имя таблицы или колонки.
- Для изображений поместите файлы в папку img или укажите свою папку в imageFolder.
- Если файл изображения отсутствует, добавьте default.png в папку изображений.
- Обрабатывайте исключения на уровне интерфейса или сервиса, чтобы показать пользователю понятное сообщение.
