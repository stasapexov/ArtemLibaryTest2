using ArtemLibaryTest.Models;
using ArtemLibaryTest.QuickStart;
using SampleWpf.Pages;

namespace SampleWpf;

public sealed class SampleMenuProvider : IMenuProvider
{
    public IEnumerable<NavMenuItem> GetMenuItems(Users currentUser)
    {
        return
        [
            new NavMenuItem
            {
                Title = "Магазин",
                Tag = "Shop",
                Icon = "Shop",
                Roles = ["admin", "manager", "user", "guest"],
                CreatePage = () => new ShopPage()
            },
            new NavMenuItem
            {
                Title = "Мои заказы",
                Tag = "MyOrders",
                Icon = "List",
                Roles = ["admin", "manager", "user"],
                CreatePage = () => new MyOrdersPage()
            },
            new NavMenuItem
            {
                Title = "Все заказы",
                Tag = "AdminOrders",
                Icon = "Admin",
                Roles = ["admin", "manager"],
                CreatePage = () => new AdminOrdersPage()
            }
        ];
    }
}
