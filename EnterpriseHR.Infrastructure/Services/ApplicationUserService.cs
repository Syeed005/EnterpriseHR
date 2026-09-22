using EnterpriseHR.Core.Entities;
using EnterpriseHR.Core.Security;
using EnterpriseHR.Core.Services;
using EnterpriseHR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.Services {
    public class ApplicationUserService : IApplicationUserService {
        private readonly EnterpriseHrDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;

        public ApplicationUserService(EnterpriseHrDbContext dbContext, ICurrentUserService currentUserService) {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
        }

        public async Task<ApplicationUser> GetOrCreateCurrentUserAsync() {
            if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.ExternalUserId))
                throw new UnauthorizedAccessException("An authenticated user identity is required.");

            var externalUserId = _currentUserService.ExternalUserId;

            var existingUser = await _dbContext.ApplicationUsers.SingleOrDefaultAsync(x => x.ExternalUserId == externalUserId);

            if (existingUser != null)
                return existingUser;

            var user = new ApplicationUser {
                ApplicationUserId = Guid.NewGuid(),
                ExternalUserId = externalUserId,
                Email = _currentUserService.Email ?? string.Empty,
                DisplayName = _currentUserService.DisplayName ?? string.Empty,
                Role = ApplicationRoles.Employee,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.ApplicationUsers.Add(user);
            await _dbContext.SaveChangesAsync();

            return user;
        }
    }
}
