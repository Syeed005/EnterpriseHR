using EnterpriseHR.Core.Security;
using System.Security.Claims;

namespace EnterpriseHR.Api.Security {
    public class CurrentUserService : ICurrentUserService {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        }
        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

        public string? ExternalUserId =>
        User?.FindFirst("oid")?.Value ??
        User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
        User?.FindFirst("sub")?.Value;

        public string? Email =>
            User?.FindFirst(ClaimTypes.Email)?.Value ??
            User?.FindFirst("preferred_username")?.Value;

        public string? DisplayName =>
            User?.FindFirst(ClaimTypes.Name)?.Value ??
            User?.FindFirst("name")?.Value;
    }
}
