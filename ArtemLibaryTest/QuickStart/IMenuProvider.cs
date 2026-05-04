using ArtemLibaryTest.Models;

namespace ArtemLibaryTest.QuickStart
{
    public interface IMenuProvider
    {
        // ВАЖНО: Tag каждого NavMenuItem должен быть уникальным.
        IEnumerable<NavMenuItem> GetMenuItems(Users currentUser);
    }
}
