using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Evaluation.Retrieval {
    public interface IRetrievalEvaluationService {
        Task<RetrievalEvaluationSummary> RunAsync(int topK = 3);
    }
}
