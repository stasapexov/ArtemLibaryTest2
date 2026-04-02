using System.Windows;
using System.Windows.Controls;

namespace ArtemLibaryTest.QuickStart
{
    public class DefaultHomePage : Page
    {
        public DefaultHomePage(string text)
        {
            Content = new TextBlock
            {
                Text = text,
                FontSize = 24,
                Margin = new Thickness(20)
            };
        }
    }
}
