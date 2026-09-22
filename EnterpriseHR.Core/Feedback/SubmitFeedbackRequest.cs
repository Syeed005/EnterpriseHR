using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Feedback {
    public class SubmitFeedbackRequest {
        public long ChatMessageId { get; set; }
        public bool IsHelpful { get; set; }
        public string? Comment { get; set; }
    }
}
