using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Tools {
    public class ToolCallingResult {
        public string Answer { get; set; } = string.Empty;
        public bool ToolUsed { get; set; }
        public string? ToolName { get; set; }
    }
}
