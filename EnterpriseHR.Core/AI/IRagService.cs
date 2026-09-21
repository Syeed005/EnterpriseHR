using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.AI {
    public interface IRagService {
        Task<RagAnswer> AskAsync(Guid sessionId, string question, int topK = 3);
    }
}
