using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.AI {
    public class RagQuestionRequest {
        public Guid SessionId { get; set; }
        public string Question { get; set; } = string.Empty;
        public int TopK { get; set; } = 3;
    }
}
