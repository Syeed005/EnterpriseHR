using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Entities {
    public class AnswerFeedback {
        public long AnswerFeedbackId { get; set; }
        public long ChatMessageId { get; set; }
        public Guid ApplicationUserId { get; set; }
        public bool IsHelpful { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public ChatMessage ChatMessage { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
