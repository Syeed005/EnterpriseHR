using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Evaluation {
    public interface IRetrievalEvaluationService {
        Task<IReadOnlyList<RetrievalEvaluationResult>> RunAsync(int topK = 3);
    }
}
