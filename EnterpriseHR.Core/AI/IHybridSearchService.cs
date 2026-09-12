using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.AI {
    public interface IHybridSearchService {
        Task<IReadOnlyList<RetrievalResult>> SearchAsync(string query, int topK = 3);
    }
}
