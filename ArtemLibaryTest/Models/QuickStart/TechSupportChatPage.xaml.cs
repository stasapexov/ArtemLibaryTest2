using ArtemLibaryTest.Core;
using ArtemLibaryTest.Models.QuickStart;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArtemLibaryTest.QuickStart
{
    public partial class TechSupportChatPage : Page
    {
        private readonly int _conversationId;
        private readonly bool _isSupportSide;
        private readonly Action? _messageSent;

        public TechSupportChatPage(int conversationId, bool isSupportSide, Action? messageSent = null)
        {
            InitializeComponent();
            _conversationId = conversationId;
            _isSupportSide = isSupportSide;
            _messageSent = messageSent;
            LoadChat();
        }

        private void LoadChat()
        {
            var conversation = SupportStore.GetConversation(_conversationId);
            if (conversation == null)
            {
                ChatTitleText.Text = "Чат не найден";
                MessageInput.IsEnabled = false;
                return;
            }

            ChatTitleText.Text = _isSupportSide
                ? $"Клиент: {conversation.ClientName} ({conversation.ClientLogin})"
                : "Чат с техподдержкой";

            RenderMessages(conversation);
        }

        private void RenderMessages(SupportConversation conversation)
        {
            MessagesPanel.Children.Clear();

            foreach (var message in conversation.Messages)
            {
                var senderLabel = _isSupportSide && !message.IsFromSupport
                    ? $"Клиент {message.SenderName}"
                    : message.IsFromSupport
                        ? $"Техподдержка: {message.SenderName}"
                        : message.SenderName;

                var messageBlock = new TextBlock
                {
                    Text = $"{senderLabel} • {message.SentAt:dd.MM.yyyy HH:mm}\n{message.Text}",
                    TextWrapping = TextWrapping.Wrap
                };

                var border = new Border
                {
                    Background = message.IsFromSupport ? new SolidColorBrush(Color.FromRgb(232, 245, 255)) : new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10),
                    Margin = new Thickness(0, 0, 0, 8),
                    Child = messageBlock,
                    HorizontalAlignment = message.IsFromSupport == _isSupportSide ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                    MaxWidth = 420
                };

                MessagesPanel.Children.Add(border);
            }

            MessagesScrollViewer.ScrollToEnd();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (Session.CurrentUser == null)
            {
                MessageBox.Show("Пользователь не найден.");
                return;
            }

            var text = MessageInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                MessageBox.Show("Введите сообщение.");
                return;
            }

            if (_isSupportSide)
            {
                var message = SupportStore.AddSupportMessage(_conversationId, Session.CurrentUser, text);
                if (message == null)
                {
                    MessageBox.Show("Не удалось отправить сообщение.");
                    return;
                }
            }
            else
            {
                SupportStore.AddClientMessage(Session.CurrentUser, text);
            }

            MessageInput.Clear();
            LoadChat();
            _messageSent?.Invoke();
        }
    }
}