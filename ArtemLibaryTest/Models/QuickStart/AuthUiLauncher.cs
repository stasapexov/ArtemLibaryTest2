using ArtemLibaryTest.Core;
using System.Windows;

namespace ArtemLibaryTest.QuickStart
{
    public static class AuthUiLauncher
    {
        public static Window CreateLoginWindow(IAuthService authService, AuthUiOptions? options = null)
        {
            var context = new AuthUiContext(authService, options ?? new AuthUiOptions());
            return new ReadyLoginWindow(context);
        }
    }
}
