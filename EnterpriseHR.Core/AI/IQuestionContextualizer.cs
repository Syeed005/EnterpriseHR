using EnterpriseHR.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.AI {
    public interface IQuestionContextualizer {
        Task<string> ContextualizeAsync(string question, IReadOnlyList<ChatMessage> history);
    }
}
