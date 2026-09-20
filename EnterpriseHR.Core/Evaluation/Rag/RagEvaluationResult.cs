using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Evaluation.Rag {
    public class RagEvaluationResult {
        public string Id { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;

        public string Answer { get; set; } = string.Empty;

        public List<string> ExpectedAnswerFacts { get; set; } = [];
        public List<string> MatchedFacts { get; set; } = [];
        public List<string> MissingFacts { get; set; } = [];

        public bool AllExpectedFactsFound { get; set; }

        public int ExpectedDocumentChunkId { get; set; }
        public bool ExpectedCitationFound { get; set; }

        public long LatencyMs { get; set; }
    }
}
