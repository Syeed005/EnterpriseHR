using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Evaluation.Conversation {
    public class ConversationEvaluationTurn {
        public string Id { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public List<string> ExpectedAnswerFacts { get; set; } = [];
        public int ExpectedDocumentChunkId { get; set; }
    }
}
