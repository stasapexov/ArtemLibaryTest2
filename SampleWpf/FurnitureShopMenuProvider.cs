using System.Collections.Generic;
using ArtemLibaryTest.Models;
using ArtemLibaryTest.QuickStart;

namespace SampleWpf;

public sealed class FurnitureShopMenuProvider : IMenuProvider
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
                Roles = ["user", "guest"],
                CreatePage = () => new ShopPage()
            },
            new NavMenuItem
            {
                Title = "Мои заказы",
                Tag = "MyOrders",
                Icon = "List",
                Roles = ["user"],
                CreatePage = () => new MyOrdersPage()
            },
            new NavMenuItem
            {
                Title = "Заказы",
                Tag = "Orders",
                Icon = "Document",
                Roles = ["admin", "manager"],
                CreatePage = () => new OrdersPage()
            },
            new NavMenuItem
            {
                Title = "Товары",
                Tag = "Products",
                Icon = "Shop",
                Roles = ["admin", "manager"],
                CreatePage = () => new ProductsPage()
            }
        ];
    }
}
