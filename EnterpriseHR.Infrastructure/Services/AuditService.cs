using EnterpriseHR.Core.Entities;
using EnterpriseHR.Core.Services;
using EnterpriseHR.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.Services {
    public class AuditService : IAuditService {
        private readonly EnterpriseHrDbContext _dbContext;
        private readonly IApplicationUserService _applicationUserService;

        public AuditService(EnterpriseHrDbContext dbContext, IApplicationUserService applicationUserService) {
            _dbContext = dbContext;
            _applicationUserService = applicationUserService;
        }

        public async Task RecordAsync(string action, string resourceType, string resourceId, string? details = null) {
            var user = await _applicationUserService.GetOrCreateCurrentUserAsync();

            var auditLog = new AuditLog {
                ApplicationUserId = user.ApplicationUserId,
                Action = action,
                ResourceType = resourceType,
                ResourceId = resourceId,
                Details = details,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();
        }
    }
}
