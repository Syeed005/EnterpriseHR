using EnterpriseHR.Core.Security;
using EnterpriseHR.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.Security {
    public class DocumentAccessService : IDocumentAccessService {
        private readonly IApplicationUserService _applicationUserService;

        public DocumentAccessService(IApplicationUserService applicationUserService) {
            _applicationUserService = applicationUserService;
        }

        public async Task<IReadOnlyList<string>> GetAllowedAccessLevelsAsync() {
            var user = await _applicationUserService.GetOrCreateCurrentUserAsync();

            return user.Role switch {
                ApplicationRoles.Employee => new[] { DocumentAccessLevels.Employee },
                ApplicationRoles.HR => new[] { DocumentAccessLevels.Employee, DocumentAccessLevels.HR},
                ApplicationRoles.Admin => new[] {DocumentAccessLevels.Employee,DocumentAccessLevels.HR,DocumentAccessLevels.Admin},
                _ => Array.Empty<string>()
            };
        }
    }
}
