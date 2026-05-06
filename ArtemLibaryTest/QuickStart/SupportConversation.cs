using System.Collections.ObjectModel;
using System.Linq;

namespace ArtemLibaryTest.QuickStart
{
    public class SupportConversation
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientLogin { get; set; } = string.Empty;
        public ObservableCollection<SupportMessage> Messages { get; } = [];
        public string DisplayText => $"{ClientName} ({ClientLogin})";
        public string LastMessageText => Messages.LastOrDefault()?.Text ?? "Нет сообщений";
    }
}
