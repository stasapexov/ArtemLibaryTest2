using ArtemLibaryTest.Core;
using ArtemLibaryTest.Models;
using System.Windows;
using System.Windows.Controls;

namespace ArtemLibaryTest.QuickStart
{
    public static class AuthUiLauncher
    {
        public static AuthUiContext CreateContext(IAuthService authService, AuthUiOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(authService);

            var resolvedOptions = options ?? new AuthUiOptions();
            var context = new AuthUiContext(authService, resolvedOptions);
            resolvedOptions.MenuProvider ??= new DefaultMenuProvider(resolvedOptions, context);

            return context;
        }

        public static Window CreateLoginWindow(IAuthService authService, AuthUiOptions? options = null)
        {
            return CreateLoginWindow(CreateContext(authService, options));
        }

        public static Window CreateLoginWindow(AuthUiContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Options.LoginWindowFactory?.Invoke(context) ?? new ReadyLoginWindow(context);
        }

        public static Window CreateRegisterWindow(IAuthService authService, AuthUiOptions? options = null)
        {
            return CreateRegisterWindow(CreateContext(authService, options));
        }

        public static Window CreateRegisterWindow(AuthUiContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Options.RegisterWindowFactory?.Invoke(context) ?? new ReadyRegisterWindow(context);
        }

        public static Window CreateMainWindow(AuthUiContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return new ReadyMainWindow(context);
        }

        public static void OpenLoginWindow(AuthUiContext context, Window? currentWindow = null)
        {
            var loginWindow = CreateLoginWindow(context);
            loginWindow.Show();
            currentWindow?.Close();
        }

        public static void OpenRegisterWindow(AuthUiContext context, Window? currentWindow = null)
        {
            var registerWindow = CreateRegisterWindow(context);
            registerWindow.Show();
            currentWindow?.Close();
        }

        public static void OpenMainWindow(AuthUiContext context, Users user, Window? currentWindow = null)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(user);

            Session.CurrentUser = user;
            CreateMainWindow(context).Show();
            currentWindow?.Close();
        }

        public static bool TryLoginAndOpenMain(
            AuthUiContext context,
            TextBox loginTextBox,
            PasswordBox passwordBox,
            Window? currentWindow = null,
            bool showMessage = true)
        {
            ArgumentNullException.ThrowIfNull(passwordBox);

            return TryLoginAndOpenMain(context, loginTextBox, passwordBox.Password, currentWindow, showMessage);
        }

        public static bool TryLoginAndOpenMain(
            AuthUiContext context,
            TextBox loginTextBox,
            TextBox passwordTextBox,
            Window? currentWindow = null,
            bool showMessage = true)
        {
            ArgumentNullException.ThrowIfNull(passwordTextBox);

            return TryLoginAndOpenMain(context, loginTextBox, passwordTextBox.Text, currentWindow, showMessage);
        }

        public static bool TryLoginAndOpenMain(
            IAuthService authService,
            TextBox loginTextBox,
            PasswordBox passwordBox,
            Window? currentWindow = null,
            bool showMessage = true)
        {
            return TryLoginAndOpenMain(authService, null, loginTextBox, passwordBox, currentWindow, showMessage);
        }

        public static bool TryLoginAndOpenMain(
            IAuthService authService,
            TextBox loginTextBox,
            TextBox passwordTextBox,
            Window? currentWindow = null,
            bool showMessage = true)
        {
            return TryLoginAndOpenMain(authService, null, loginTextBox, passwordTextBox, currentWindow, showMessage);
        }

        public static bool TryLoginAndOpenMain(
            IAuthService authService,
            AuthUiOptions? options,
            TextBox loginTextBox,
            PasswordBox passwordBox,
            Window? currentWindow = null,
            bool showMessage = true)
        {
            return TryLoginAndOpenMain(CreateContext(authService, options), loginTextBox, passwordBox, currentWindow, showMessage);
        }

        public static bool TryLoginAndOpenMain(
            IAuthService authService,
            AuthUiOptions? options,
            TextBox loginTextBox,
            TextBox passwordTextBox,
            Window? currentWindow = null,
            bool showMessage = true)
        {
            return TryLoginAndOpenMain(CreateContext(authService, options), loginTextBox, passwordTextBox, currentWindow, showMessage);
        }

        public static bool TryRegisterAndOpenLogin(
            AuthUiContext context,
            TextBox loginTextBox,
            PasswordBox passwordBox,
            TextBox nameTextBox,
            TextBox phoneTextBox,
            TextBox? emailTextBox = null,
            Window? currentWindow = null,
            bool showMessage = true)
        {
            ArgumentNullException.ThrowIfNull(passwordBox);

            return TryRegisterAndOpenLogin(
                context,
                loginTextBox,
                passwordBox.Password,
                nameTextBox,
                phoneTextBox,
                emailTextBox,
                currentWindow,
                showMessage);
        }

        public static bool TryRegisterAndOpenLogin(
            AuthUiContext context,
            TextBox loginTextBox,
            TextBox passwordTextBox,
            TextBox nameTextBox,
            TextBox phoneTextBox,
            TextBox? emailTextBox = null,
            Window? currentWindow = null,
            bool showMessage = true)
        {
            ArgumentNullException.ThrowIfNull(passwordTextBox);

            return TryRegisterAndOpenLogin(
                context,
                loginTextBox,
                passwordTextBox.Text,
                nameTextBox,
                phoneTextBox,
                emailTextBox,
                currentWindow,
                showMessage);
        }

        public static bool TryRegisterAndOpenLogin(
            IAuthService authService,
            TextBox loginTextBox,
            PasswordBox passwordBox,
            TextBox nameTextBox,
            TextBox phoneTextBox,
            TextBox? emailTextBox = null,
            Window? currentWindow = null,
            bool showMessage = true)
        {
            return TryRegisterAndOpenLogin(
                authService,
                null,
                loginTextBox,
                passwordBox,
                nameTextBox,
                phoneTextBox,
                emailTextBox,
                currentWindow,
                showMessage);
        }

        public static bool TryRegisterAndOpenLogin(
            IAuthService authService,
            TextBox loginTextBox,
            TextBox passwordTextBox,
            TextBox nameTextBox,
            TextBox phoneTextBox,
            TextBox? emailTextBox = null,
            Window? currentWindow = null,
            bool showMessage = true)
        {
            return TryRegisterAndOpenLogin(
                authService,
                null,
                loginTextBox,
                passwordTextBox,
                nameTextBox,
                phoneTextBox,
                emailTextBox,
                currentWindow,
                showMessage);
        }

        public static bool TryRegisterAndOpenLogin(
            IAuthService authService,
            AuthUiOptions? options,
            TextBox loginTextBox,
            PasswordBox passwordBox,
            TextBox nameTextBox,
            TextBox phoneTextBox,
            TextBox? emailTextBox = null,
            Window? currentWindow = null,
            bool showMessage = true)
        {
            return TryRegisterAndOpenLogin(
                CreateContext(authService, options),
                loginTextBox,
                passwordBox,
                nameTextBox,
                phoneTextBox,
                emailTextBox,
                currentWindow,
                showMessage);
        }

        public static bool TryRegisterAndOpenLogin(
            IAuthService authService,
            AuthUiOptions? options,
            TextBox loginTextBox,
            TextBox passwordTextBox,
            TextBox nameTextBox,
            TextBox phoneTextBox,
            TextBox? emailTextBox = null,
            Window? currentWindow = null,
            bool showMessage = true)
        {
            return TryRegisterAndOpenLogin(
                CreateContext(authService, options),
                loginTextBox,
                passwordTextBox,
                nameTextBox,
                phoneTextBox,
                emailTextBox,
                currentWindow,
                showMessage);
        }

        private static bool TryLoginAndOpenMain(
            AuthUiContext context,
            TextBox loginTextBox,
            string password,
            Window? currentWindow,
            bool showMessage)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(loginTextBox);

            var user = context.AuthService.Login(loginTextBox.Text.Trim(), password);
            if (user == null)
            {
                if (showMessage)
                {
                    MessageBox.Show("Неверный логин или пароль");
                }

                return false;
            }

            OpenMainWindow(context, user, currentWindow);
            return true;
        }

        private static bool TryRegisterAndOpenLogin(
            AuthUiContext context,
            TextBox loginTextBox,
            string password,
            TextBox nameTextBox,
            TextBox phoneTextBox,
            TextBox? emailTextBox,
            Window? currentWindow,
            bool showMessage)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(loginTextBox);
            ArgumentNullException.ThrowIfNull(nameTextBox);
            ArgumentNullException.ThrowIfNull(phoneTextBox);

            var success = context.AuthService.Register(
                loginTextBox.Text.Trim(),
                password,
                nameTextBox.Text.Trim(),
                phoneTextBox.Text.Trim(),
                emailTextBox?.Text.Trim() ?? string.Empty);

            if (!success)
            {
                if (showMessage)
                {
                    MessageBox.Show("Логин уже занят");
                }

                return false;
            }

            if (showMessage)
            {
                MessageBox.Show("Регистрация успешна");
            }

            OpenLoginWindow(context, currentWindow);
            return true;
        }
    }
}
