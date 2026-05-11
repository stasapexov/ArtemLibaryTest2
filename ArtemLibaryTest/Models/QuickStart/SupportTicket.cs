using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemLibaryTest.Models.QuickStart
{
    public class SupportTicket
    {
        public int Id { get; set; }
        public string UserLogin { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Response { get; set; }
        public string DisplayText => $"#{Id} [{UserLogin}] {Message}";
    }
}
