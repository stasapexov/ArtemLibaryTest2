using ArtemLibaryTest.Core;
using ModernWpf.Controls;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using ArtemLibaryTest.Models.QuickStart;
using System.Diagnostics;
using WpfPage = System.Windows.Controls.Page;

namespace ArtemLibaryTest.QuickStart
{//прив
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
            ProfileButton.Visibility = Session.IsGuest ? Visibility.Collapsed : Visibility.Visible;
            NavView.IsSettingsVisible = _context.Options.IsSettingsVisible;
            ShortCut();
            BuildMenu();
        }
        public void ShortCut()
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string shortcutLocation = System.IO.Path.Combine(desktopPath, "Demo1.lnk");

                // Получаем путь к exe (учитывая, что мы в библиотеке)
                string exePath = System.Diagnostics
                    .Process.GetCurrentProcess().MainModule.FileName;

                // Используем dynamic, чтобы избежать ошибок с System.Object
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) throw new Exception("WScript.Shell не найден");

                dynamic shell = Activator.CreateInstance(shellType);
                var shortcut = shell.CreateShortcut(shortcutLocation);

                shortcut.Description = "Моя программа";
                shortcut.TargetPath = exePath;
                shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(exePath);
                shortcut.Save();
            }
            catch (Exception ex)
            {
                // В библиотеках лучше пробрасывать исключение или логировать
                throw new Exception($"Ошибка при создании ярлыка в DLL: {ex.Message}");
            }
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

            foreach (var item in items)
            {
                if (!CanOpenForRole(item.Roles, currentUser.Status))
                {
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

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Session.Logout();
            var loginWindow = new ReadyLoginWindow(_context);
            loginWindow.Show();
            Close();
        }

        private void OpenProfile_Click(object sender, RoutedEventArgs e)
        {
            var profileWindow = new UserProfileWindow(_context)
            {
                Owner = this
            };
            profileWindow.ShowDialog();
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