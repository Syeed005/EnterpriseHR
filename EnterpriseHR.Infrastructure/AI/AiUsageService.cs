using EnterpriseHR.Core.AI;
using EnterpriseHR.Core.Entities;
using EnterpriseHR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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

        public async Task<AiUsageSummary> GetSummaryAsync() {
            var records = await _dbContext.AiUsageRecords
                .AsNoTracking()
                .ToListAsync();

            if (records.Count == 0)
                return new AiUsageSummary();

            return new AiUsageSummary {
                RequestCount = records.Count,
                TotalInputTokens = records.Sum(x => (long)x.InputTokens),
                TotalOutputTokens = records.Sum(x => (long)x.OutputTokens),
                TotalTokens = records.Sum(x => (long)x.TotalTokens),
                TotalCostUsd = records.Sum(x => x.TotalCostUsd),
                AverageCostPerRequestUsd = records.Average(x => x.TotalCostUsd)
            };
        }
    }
}
