using ArtemLibaryTest.Models;

namespace ArtemLibaryTest.QuickStart
{
    public class DefaultMenuProvider : IMenuProvider
    {
        private readonly AuthUiOptions _options;

        public DefaultMenuProvider(AuthUiOptions options)
        {
            _options = options;
        }

        public IEnumerable<NavMenuItem> GetMenuItems(Users currentUser)
        {
            return
            [
                new NavMenuItem
                {
                    Title = "Главная",
                    Tag = "Home",
                    Icon = "Home",
                    Roles = ["admin", "manager", "user"],
                    CreatePage = () => new DefaultHomePage(_options.MainWelcomeText)
                }
            ];
        }
    }
}
