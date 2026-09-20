using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Evaluation.Rag {
    public class RagEvaluationSummary {
        public int TotalCases { get; set; }
        public int PassedCases { get; set; }
        public int FailedCases { get; set; }

        public double AnswerFactSuccessRate { get; set; }
        public double CitationSuccessRate { get; set; }

        public double AverageLatencyMs { get; set; }
        public long MinLatencyMs { get; set; }
        public long MaxLatencyMs { get; set; }

        public IReadOnlyList<RagEvaluationResult> Results { get; set; } = [];
    }
}
