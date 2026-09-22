using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Security {
    public interface IDocumentAccessService {
        Task<IReadOnlyList<string>> GetAllowedAccessLevelsAsync();
    }
}
