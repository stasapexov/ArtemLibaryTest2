using ArtemLibaryTest.Core;
using ArtemLibaryTest.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ArtemLibaryTest.QuickStart
{
    public partial class UserProfilePage : Page
    {
        private readonly AuthUiContext _context;

        public UserProfilePage(AuthUiContext context)
        {
            InitializeComponent();
            _context = context;
            LoadUserInfo(Session.CurrentUser);
        }

        private void LoadUserInfo(Users? user)
        {
            if (user == null)
            {
                return;
            }

            NameText.Text = user.Name;
            LoginInput.Text = user.Login;
            PasswordInput.Password = user.Password;
            PhoneInput.Text = user.Phone;
            EmailInput.Text = string.Empty;
            MoneyText.Text = $"{user.Money:0.00} ₽";

            if (user.Img.Length == 0)
            {
                return;
            }

            using var ms = new MemoryStream(user.Img);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = ms;
            bitmap.EndInit();
            AvatarImage.Source = bitmap;
        }

        private void TopUp_Click(object sender, RoutedEventArgs e)
        {
            var topUpWindow = new TopUpMoneyWindow(_context);
            var result = topUpWindow.ShowDialog();
            if (result == true)
            {
                LoadUserInfo(Session.CurrentUser);
            }
        }

        private void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            if (Session.CurrentUser == null)
            {
                MessageBox.Show("Пользователь не найден.");
                return;
            }

            Session.CurrentUser.Login = LoginInput.Text.Trim();
            Session.CurrentUser.Password = PasswordInput.Password;
            Session.CurrentUser.Phone = PhoneInput.Text.Trim();

            MessageBox.Show("Изменения сохранены в текущей сессии.");
        }
    }
}
