using ArtemLibaryTest.Core;
using System.Windows;

namespace ArtemLibaryTest.QuickStart
{
    public static class AuthUiLauncher
    {
        public static Window CreateLoginWindow(IAuthService authService, AuthUiOptions? options = null)
        {
            var resolvedOptions = options ?? new AuthUiOptions();
            resolvedOptions.MenuProvider ??= new DefaultMenuProvider(resolvedOptions);

            var context = new AuthUiContext(authService, resolvedOptions);
            return new ReadyLoginWindow(context);
        }
    }
}
