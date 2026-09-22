using EnterpriseHR.Core.Services;
using Microsoft.AspNetCore.Authorization;

namespace EnterpriseHR.Api.Security {
    public class ApplicationRoleAuthorizationHandler : AuthorizationHandler<ApplicationRoleRequirement> {
        private readonly IApplicationUserService _applicationUserService;

        public ApplicationRoleAuthorizationHandler(IApplicationUserService applicationUserService) {
            _applicationUserService = applicationUserService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ApplicationRoleRequirement requirement) {
            if (context.User.Identity?.IsAuthenticated != true)
                return;

            var user = await _applicationUserService.GetOrCreateCurrentUserAsync();

            if (requirement.AllowedRoles.Contains(user.Role, StringComparer.OrdinalIgnoreCase))
                context.Succeed(requirement);
        }
    }
}
