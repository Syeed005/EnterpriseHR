using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.AI {
    public class RagQuestionRequest {
        public string Question { get; set; } = string.Empty;
        public int TopK { get; set; } = 3;
    }
}
