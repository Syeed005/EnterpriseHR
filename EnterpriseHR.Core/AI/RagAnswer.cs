using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.AI {
    public class RagAnswer {
        public string Answer { get; set; } = string.Empty;
        public IReadOnlyList<RagCitation> Citations { get; set; } = [];
    }
}
