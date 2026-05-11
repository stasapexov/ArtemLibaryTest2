using ArtemLibaryTest.Core;

namespace ArtemLibaryTest.QuickStart
{
    public sealed class AuthUiContext
    {
        public AuthUiContext(IAuthService authService, AuthUiOptions options)
        {
            AuthService = authService;
            Options = options;
        }

        public IAuthService AuthService { get; }
        public AuthUiOptions Options { get; }
    }
}