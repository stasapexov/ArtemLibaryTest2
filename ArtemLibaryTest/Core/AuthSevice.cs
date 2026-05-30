using ArtemLibaryTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemLibaryTest.Core
{
    /// <summary>
    /// Контракт сервиса авторизации для готовых окон входа/регистрации и пользовательского профиля.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>Проверяет логин и пароль и возвращает пользователя или null.</summary>
        Users? Login(string login, string password);
        /// <summary>Регистрирует нового пользователя; возвращает false, если логин уже занят.</summary>
        bool Register(string login, string password, string name, string phone, string email = "");
        /// <summary>Обновляет профиль пользователя.</summary>
        bool UpdateProfile(int userId, string login, string password, string phone, string email);
        /// <summary>Сохраняет аватар пользователя в BLOB-колонку img.</summary>
        bool UpdateAvatar(int userId, byte[] imageBytes);
    }
}