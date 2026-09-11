using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.AI {
    public interface IFullTextSearchService {
        Task<IReadOnlyList<FullTextSearchResult>> SearchAsync(string query, int topK = 5);

    }
}