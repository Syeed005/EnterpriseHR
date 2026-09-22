using EnterpriseHR.Core.Entities;
using EnterpriseHR.Core.Services;
using EnterpriseHR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.Services {
    public class EmployeeProfileService : IEmployeeProfileService {
        private readonly EnterpriseHrDbContext _dbContext;
        private readonly IApplicationUserService _applicationUserService;

        public EmployeeProfileService(EnterpriseHrDbContext dbContext, IApplicationUserService applicationUserService) {
            _dbContext = dbContext;
            _applicationUserService = applicationUserService;
        }

        public async Task<EmployeeProfile?> GetMyProfileAsync() {
            var user = await _applicationUserService.GetOrCreateCurrentUserAsync();

            return await _dbContext.EmployeeProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.ApplicationUserId == user.ApplicationUserId);
        }
    }
}
