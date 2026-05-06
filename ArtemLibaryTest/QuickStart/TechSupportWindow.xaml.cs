using ArtemLibaryTest.Core;
using System.Windows;
using System.Windows.Controls;

namespace ArtemLibaryTest.QuickStart
{
    public partial class TechSupportWindow : Window
    {
        private int? _selectedConversationId;

        public TechSupportWindow()
        {
            InitializeComponent();
            InitializeMode();
        }

        private void InitializeMode()
        {
            if (Session.CurrentUser == null)
            {
                MessageBox.Show("Пользователь не найден.");
                Close();
                return;
            }

            var role = Session.CurrentUser.Status.ToLowerInvariant();
            if (role == "admin" || role == "manager")
            {
                ModeText.Text = "Техподдержка: диалоги с клиентами";
                StaffPanel.Visibility = Visibility.Visible;
                LoadClients();
                return;
            }

            ModeText.Text = "Чат с техподдержкой";
            ClientChatFrame.Visibility = Visibility.Visible;
            var conversation = SupportStore.GetOrCreateConversation(Session.CurrentUser);
            ClientChatFrame.Navigate(new TechSupportChatPage(conversation.Id, false));
        }

        private void LoadClients()
        {
            ClientsPanel.Children.Clear();
            var conversations = SupportStore.GetClientConversations();

            if (conversations.Count == 0)
            {
                ClientsPanel.Children.Add(new TextBlock
                {
                    Text = "Пока нет обращений.",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = System.Windows.Media.Brushes.Gray
                });
                return;
            }

            foreach (var conversation in conversations)
            {
                var button = new Button
                {
                    Content = $"{conversation.DisplayText}\n{conversation.LastMessageText}",
                    Tag = conversation.Id,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 0, 0, 8),
                    Padding = new Thickness(8)
                };
                button.Click += ClientButton_Click;
                ClientsPanel.Children.Add(button);
            }

            if (_selectedConversationId.HasValue)
            {
                OpenConversation(_selectedConversationId.Value);
            }
        }

        private void ClientButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not int conversationId)
            {
                return;
            }

            OpenConversation(conversationId);
        }

        private void OpenConversation(int conversationId)
        {
            _selectedConversationId = conversationId;
            StaffChatFrame.Navigate(new TechSupportChatPage(conversationId, true, LoadClients));
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
