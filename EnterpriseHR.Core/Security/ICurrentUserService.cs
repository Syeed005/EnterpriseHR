using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Security {
    public interface ICurrentUserService {
        bool IsAuthenticated { get; }
        string? ExternalUserId { get; }
        string? Email { get; }
        string? DisplayName { get; }
    }
}
