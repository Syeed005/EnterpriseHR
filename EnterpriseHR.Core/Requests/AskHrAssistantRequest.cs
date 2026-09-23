using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Requests {
    public class AskHrAssistantRequest {
        public Guid SessionId { get; set; }
        public string Question { get; set; } = string.Empty;
    }
}
