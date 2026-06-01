using System;
using System.Windows;

namespace ArtemLibaryTest.QuickStart
{
    public class AuthUiOptions
    {
        public string AppTitle { get; set; } = "Приложение";
        public string MainWelcomeText { get; set; } = "Вы вошли в систему";
        public IMenuProvider? MenuProvider { get; set; }
        public bool IsSettingsVisible { get; set; }

        /// <summary>
        /// Фабрика пользовательского окна входа. Если задана, AuthUiLauncher.CreateLoginWindow вернет окно из проекта-потребителя.
        /// </summary>
        public Func<AuthUiContext, Window>? LoginWindowFactory { get; set; }

        /// <summary>
        /// Фабрика пользовательского окна регистрации. Если задана, переход на регистрацию откроет окно из проекта-потребителя.
        /// </summary>
        public Func<AuthUiContext, Window>? RegisterWindowFactory { get; set; }
    }
}
