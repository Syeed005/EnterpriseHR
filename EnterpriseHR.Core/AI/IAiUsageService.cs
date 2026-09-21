using EnterpriseHR.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.AI {
    public interface IAiUsageService {
        Task RecordAsync(AiUsageRecord record);
        Task<AiUsageSummary> GetSummaryAsync();
    }
}
