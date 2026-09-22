using Microsoft.AspNetCore.Authorization;

namespace EnterpriseHR.Api.Security {
    public class ApplicationRoleRequirement : IAuthorizationRequirement {
        public IReadOnlyCollection<string> AllowedRoles { get; }

        public ApplicationRoleRequirement(params string[] allowedRoles) {
            AllowedRoles = allowedRoles;
        }
    }
}
