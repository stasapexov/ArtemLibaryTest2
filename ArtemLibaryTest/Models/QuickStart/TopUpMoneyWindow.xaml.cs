using ArtemLibaryTest.Core;
using System.Globalization;
using System.Windows;

namespace ArtemLibaryTest.QuickStart
{
    public partial class TopUpMoneyWindow : Window
    {
        private readonly AuthUiContext _context;

        public TopUpMoneyWindow(AuthUiContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private void TopUp_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CardNumberBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Заполни номер карты и пароль.");
                return;
            }

            if (!double.TryParse(AmountBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
            {
                MessageBox.Show("Неверная сумма.");
                return;
            }

            if (Session.CurrentUser == null)
            {
                MessageBox.Show("Пользователь не найден в сессии.");
                return;
            }

            if (_context.AuthService is not MySqlAuthService mysqlAuthService)
            {
                MessageBox.Show("Пополнение поддерживается только для MySqlAuthService.");
                return;
            }

            var success = mysqlAuthService.TopUpUserMoney(Session.CurrentUser.Id, PasswordBox.Password, amount);
            if (!success)
            {
                MessageBox.Show("Не удалось пополнить. Проверь пароль.");
                return;
            }

            Session.CurrentUser.Money += amount;
            MessageBox.Show("Баланс пополнен.");
            DialogResult = true;
            Close();
        }
    }
}