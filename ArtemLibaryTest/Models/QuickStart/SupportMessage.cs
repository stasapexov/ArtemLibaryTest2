using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemLibaryTest.Models.QuickStart
{
    public class SupportMessage
    {
        public int Id { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderRole { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsFromSupport => SenderRole.Equals("admin", StringComparison.OrdinalIgnoreCase)
            || SenderRole.Equals("manager", StringComparison.OrdinalIgnoreCase);
    }
}
