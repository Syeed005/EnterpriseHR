using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Entities {
    public class ChatMessage {
        public long ChatMessageId { get; set; }

        public Guid ChatSessionId { get; set; }

        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }

        public ChatSession ChatSession { get; set; } = null!;
    }
}
