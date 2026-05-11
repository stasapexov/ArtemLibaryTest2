using ArtemLibaryTest.Models;

namespace ArtemLibaryTest.QuickStart
{
    public interface IMenuProvider
    {
        IEnumerable<NavMenuItem> GetMenuItems(Users currentUser);
    }
}
