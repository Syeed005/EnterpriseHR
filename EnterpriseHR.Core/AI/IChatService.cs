using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.AI {
    public interface IChatService {
        Task<ChatGenerationResult> GenerateAnswerAsync(string prompt);
    }
}
