using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Evaluation.Retrieval {
    public class RetrievalEvaluationCase {
        public string Id { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public int ExpectedDocumentChunkId { get; set; }
        public string? ExpectedSectionTitle { get; set; }
    }
}
