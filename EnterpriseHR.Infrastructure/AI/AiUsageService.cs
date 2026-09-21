using EnterpriseHR.Core.AI;
using EnterpriseHR.Core.Entities;
using EnterpriseHR.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.AI {
    public class AiUsageService : IAiUsageService {
        private readonly EnterpriseHrDbContext _dbContext;

        public AiUsageService(EnterpriseHrDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task RecordAsync(AiUsageRecord record) {
            _dbContext.AiUsageRecords.Add(record);
            await _dbContext.SaveChangesAsync();
        }
    }
}
