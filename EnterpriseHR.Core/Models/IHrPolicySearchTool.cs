using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Models {
    public interface IHrPolicySearchTool {
        Task<IReadOnlyList<HrPolicySearchResult>> SearchAsync(string query, int topK = 3);
    }
}
