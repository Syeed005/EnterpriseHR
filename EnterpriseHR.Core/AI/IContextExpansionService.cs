using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.AI {
    public interface IContextExpansionService {
        Task<IReadOnlyList<RetrievalResult>> ExpandAsync(IReadOnlyList<RetrievalResult> results);
    }
}
