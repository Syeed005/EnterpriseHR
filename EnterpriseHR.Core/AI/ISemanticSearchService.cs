using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.AI {
    public interface ISemanticSearchService {
        Task<IReadOnlyList<SemanticSearchResult>> SearchAsync(string query, int topK = 3);
    }
}
