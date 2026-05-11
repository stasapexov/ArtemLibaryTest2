using System.Windows.Controls;

namespace ArtemLibaryTest.QuickStart
{
    public class NavMenuItem
    {
        public string Title { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public string Icon { get; set; } = "Page";
        public string[] Roles { get; set; } = [];
        public Func<Page> CreatePage { get; set; } = () => new Page();
    }
}