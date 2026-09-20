using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Evaluation {
    public class RetrievalEvaluationSummary {
        public int TopK { get; set; }
        public int TotalCases { get; set; }
        public int PassedCases { get; set; }
        public int FailedCases { get; set; }
        public double HitRateAtK { get; set; }
        public double Top1HitRate { get; set; }
        public double MeanReciprocalRank { get; set; }
        public IReadOnlyList<RetrievalEvaluationResult> Results { get; set; } = [];
    }
}
