using ArtemLibaryTest.Core;
using ArtemLibaryTest.Models;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

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

            AvatarImage.Source = CreateBitmapFromBytes(user.Img);
        }


        private static BitmapImage CreateBitmapFromBytes(byte[] imageBytes)
        {
            using var ms = new MemoryStream(imageBytes);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = ms;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
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
            Session.CurrentUser.Phone = EmailInput.Text.Trim();

            var profileUpdated = _context.AuthService.UpdateProfile(
                Session.CurrentUser.Id,
                Session.CurrentUser.Login,
                Session.CurrentUser.Password,
                Session.CurrentUser.Phone,
                Session.CurrentUser.Email);

            if (!profileUpdated)
            {
                MessageBox.Show("Не удалось сохранить изменения в базе данных.");
                return;
            }

            MessageBox.Show("Изменения сохранены в базе данных.");
        }
        private void ChangeAvatar_Click(object sender, RoutedEventArgs e)
        {
            if (Session.CurrentUser == null)
            {
                MessageBox.Show("Пользователь не найден.");
                return;
            }

            var dialog = new OpenFileDialog
            {
                Title = "Выберите изображение",
                Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var imageBytes = File.ReadAllBytes(dialog.FileName);
            var avatarUpdated = _context.AuthService.UpdateAvatar(Session.CurrentUser.Id, imageBytes);
            if (!avatarUpdated)
            {
                MessageBox.Show("Не удалось обновить аватар в базе данных.");
                return;
            }

            Session.CurrentUser.Img = imageBytes;
            AvatarImage.Source = CreateBitmapFromBytes(imageBytes);
            MessageBox.Show("Аватар обновлён в базе данных.");
        }

    }
}