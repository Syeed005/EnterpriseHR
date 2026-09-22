using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Services {
    public interface IAuditService {
        Task RecordAsync(string action, string resourceType, string resourceId, string? details = null);
    }
}
