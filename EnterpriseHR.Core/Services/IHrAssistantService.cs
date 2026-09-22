using EnterpriseHR.Core.AI;
using EnterpriseHR.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Services {
    public interface IHrAssistantService {
        Task<HrAssistantResult> AskAsync(Guid sessionId, string question);
    }
}
