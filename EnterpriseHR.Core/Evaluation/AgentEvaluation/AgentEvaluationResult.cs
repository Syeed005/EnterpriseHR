using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Evaluation.AgentEvaluation {
    public class AgentEvaluationResult {
        public string Name { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public bool ToolRoutingPassed { get; set; }
        public bool AnswerPassed { get; set; }
        public bool CitationPassed { get; set; }
        public string Answer { get; set; } = string.Empty;
        public List<string> ToolsUsed { get; set; } = [];
    }
}
