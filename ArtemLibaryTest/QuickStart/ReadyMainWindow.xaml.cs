using ArtemLibaryTest.Core;
using ModernWpf.Controls;
using System.Windows;
using WpfPage = System.Windows.Controls.Page;

namespace ArtemLibaryTest.QuickStart
{
    public partial class ReadyMainWindow : Window
    {
        private readonly AuthUiContext _context;
        private readonly Dictionary<string, Func<WpfPage>> _pageFactoryByTag = new();

        internal ReadyMainWindow(AuthUiContext context)
        {
            InitializeComponent();
            _context = context;
            Title = $"{_context.Options.AppTitle} - Главное меню";
            HeaderText.Text = _context.Options.MainWelcomeText;
            UserInfoText.Text = $"Роль: {Session.CurrentUser?.Status}";
            NavView.IsSettingsVisible = _context.Options.IsSettingsVisible;

            BuildMenu();
        }

        private void BuildMenu()
        {
            var currentUser = Session.CurrentUser;
            if (currentUser == null)
            {
                return;
            }

            var menuProvider = _context.Options.MenuProvider ?? new DefaultMenuProvider(_context.Options, _context);
            var items = menuProvider.GetMenuItems(currentUser);

            NavigationViewItem? firstItem = null;
            var usedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in items)
            {
                if (!CanOpenForRole(item.Roles, currentUser.Status))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.Tag))
                {
                    continue;
                }

                if (!usedTags.Add(item.Tag))
                {
                    MessageBox.Show($"Дублирующийся Tag в меню: {item.Tag}. У каждого пункта должен быть уникальный Tag.", "Ошибка конфигурации меню");
                    continue;
                }

                var navItem = new NavigationViewItem
                {
                    Content = item.Title,
                    Tag = item.Tag,
                    Icon = new SymbolIcon(ParseSymbol(item.Icon))
                };

                NavView.MenuItems.Add(navItem);
                _pageFactoryByTag[item.Tag] = item.CreatePage;

                firstItem ??= navItem;
            }

            if (firstItem != null)
            {
                NavView.SelectedItem = firstItem;
                NavigateToTag(firstItem.Tag?.ToString());
            }
        }

        private static bool CanOpenForRole(IEnumerable<string> roles, string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return false;
            }

            return roles.Any(r => string.Equals(r, status, StringComparison.OrdinalIgnoreCase));
        }

        private static Symbol ParseSymbol(string icon)
        {
            return Enum.TryParse(icon, true, out Symbol parsed) ? parsed : Symbol.Page;
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer is not NavigationViewItem selectedItem)
            {
                return;
            }

            NavigateToTag(selectedItem.Tag?.ToString());
        }

        private void NavigateToTag(string? tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return;
            }

            if (!_pageFactoryByTag.TryGetValue(tag, out var createPage))
            {
                return;
            }

            ContentFrame.Navigate(createPage());
        }


        private void OpenProfile_Click(object sender, RoutedEventArgs e)
        {
            var profileWindow = new UserProfileWindow(_context)
            {
                Owner = this
            };
            profileWindow.ShowDialog();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Session.Logout();
            var loginWindow = new ReadyLoginWindow(_context);
            loginWindow.Show();
            Close();
        }

        private void OpenSupport_Click(object sender, RoutedEventArgs e)
        {
            var supportWindow = new TechSupportWindow
            {
                Owner = this
            };
            supportWindow.ShowDialog();
        }
    }
}
