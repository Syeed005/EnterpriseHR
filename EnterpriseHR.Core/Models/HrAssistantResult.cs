using EnterpriseHR.Core.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Models {
    public class HrAssistantResult {
        public string Answer { get; set; } = string.Empty;
        public long AssistantMessageId { get; set; }
        public List<RagCitation> Citations { get; set; } = [];
        public List<string> ToolsUsed { get; set; } = [];
    }
}
