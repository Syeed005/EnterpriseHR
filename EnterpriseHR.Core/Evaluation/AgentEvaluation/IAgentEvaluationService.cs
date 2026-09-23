using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Evaluation.AgentEvaluation {
    public interface IAgentEvaluationService {
        Task<IReadOnlyList<AgentEvaluationResult>> RunAsync(Guid sessionId, CancellationToken cancellationToken = default);
    }
}
