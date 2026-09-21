using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Evaluation.Conversation {
    public class ConversationEvaluationSummary {
        public int TotalTurns { get; set; }
        public int PassedTurns { get; set; }
        public int FailedTurns { get; set; }

        public double AnswerFactSuccessRate { get; set; }
        public double CitationSuccessRate { get; set; }

        public double AverageLatencyMs { get; set; }
        public long MinLatencyMs { get; set; }
        public long MaxLatencyMs { get; set; }

        public IReadOnlyList<ConversationEvaluationResult> Results { get; set; } = [];
    }
}
