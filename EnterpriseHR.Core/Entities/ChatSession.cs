using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Entities {
    public class ChatSession {
        public Guid ChatSessionId { get; set; }

        public Guid? ApplicationUserId { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime LastActivityAtUtc { get; set; }

        public ApplicationUser? ApplicationUser { get; set; } = null!;
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
