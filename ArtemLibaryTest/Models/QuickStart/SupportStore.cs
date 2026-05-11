using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemLibaryTest.Models.QuickStart
{
    public static class SupportStore
    {
        private static readonly ObservableCollection<SupportConversation> _conversations = [];
        private static int _nextConversationId = 1;
        private static int _nextMessageId = 1;

        public static IReadOnlyList<SupportConversation> GetClientConversations() => _conversations
            .Where(c => c.Messages.Count > 0)
            .OrderByDescending(c => c.Messages.Last().SentAt)
            .ToList();

        public static SupportConversation GetOrCreateConversation(Users client)
        {
            var conversation = _conversations.FirstOrDefault(c => c.ClientId == client.Id);
            if (conversation != null)
            {
                conversation.ClientName = client.Name;
                conversation.ClientLogin = client.Login;
                return conversation;
            }

            conversation = new SupportConversation
            {
                Id = _nextConversationId++,
                ClientId = client.Id,
                ClientName = client.Name,
                ClientLogin = client.Login
            };

            _conversations.Add(conversation);
            return conversation;
        }

        public static SupportConversation? GetConversation(int conversationId)
        {
            return _conversations.FirstOrDefault(c => c.Id == conversationId);
        }

        public static SupportMessage? AddClientMessage(Users client, string text)
        {
            var conversation = GetOrCreateConversation(client);
            return AddMessage(conversation, client.Name, client.Status, text);
        }

        public static SupportMessage? AddSupportMessage(int conversationId, Users supportUser, string text)
        {
            var conversation = GetConversation(conversationId);
            if (conversation == null)
            {
                return null;
            }

            return AddMessage(conversation, supportUser.Name, supportUser.Status, text);
        }

        private static SupportMessage AddMessage(SupportConversation conversation, string senderName, string senderRole, string text)
        {
            var message = new SupportMessage
            {
                Id = _nextMessageId++,
                SenderName = senderName,
                SenderRole = senderRole,
                Text = text,
                SentAt = DateTime.Now
            };

            conversation.Messages.Add(message);
            return message;
        }
    }
}
