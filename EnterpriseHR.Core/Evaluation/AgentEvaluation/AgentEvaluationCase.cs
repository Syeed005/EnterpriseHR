using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Evaluation.AgentEvaluation {
    public class AgentEvaluationCase {
        public string Name { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public List<string> ExpectedTools { get; set; } = [];
        public List<string> RequiredAnswerTerms { get; set; } = [];
        public List<string> ForbiddenAnswerTerms { get; set; } = [];
        public bool RequiresCitation { get; set; }
    }
}
