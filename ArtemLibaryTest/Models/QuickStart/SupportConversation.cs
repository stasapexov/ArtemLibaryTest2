using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemLibaryTest.Models.QuickStart
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
