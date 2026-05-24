using ArtemLibaryTest.Models;

namespace ArtemLibaryTest.QuickStart
{
    public class DefaultMenuProvider : IMenuProvider
    {
        private readonly AuthUiOptions _options;
        private readonly AuthUiContext _context;

        public DefaultMenuProvider(AuthUiOptions options, AuthUiContext context)
        {
            _options = options;
            _context = context;
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
                     },
                new NavMenuItem
                {
                    Title = "Профиль",
                    Tag = "Profile",
                    Icon = "Contact",
                    Roles = ["admin", "manager", "user"],
                    CreatePage = () => new UserProfilePage(_context)
                },
                new NavMenuItem
                {
                    Title = "Заказ товаров",
                    Tag = "ProductsOrder",
                    Icon = "Shop",
                    Roles = ["admin", "manager", "user"],
                    CreatePage = () => new ProductsOrderPage(_context)
                },
                new NavMenuItem
                {
                    Title = "Мои заказы",
                    Tag = "MyOrders",
                    Icon = "AllApps",
                    Roles = ["admin", "manager", "user"],
                    CreatePage = () => new MyOrdersPage(_context)
                }
            ];
        }
    }
}
