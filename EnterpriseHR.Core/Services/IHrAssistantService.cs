using EnterpriseHR.Core.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Services {
    public interface IHrAssistantService {
        Task<RagAnswer> AskAsync(string question, Guid chatSessionId);
    }
}
