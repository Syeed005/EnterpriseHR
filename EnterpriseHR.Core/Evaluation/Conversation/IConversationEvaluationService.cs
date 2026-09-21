using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Evaluation.Conversation {
    public interface IConversationEvaluationService {
        Task<ConversationEvaluationSummary> RunAsync();
    }
}
