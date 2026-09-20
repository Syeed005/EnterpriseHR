using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Evaluation.Rag {
    public interface IRagEvaluationService {
        Task<RagEvaluationSummary> RunAsync();
    }
}
