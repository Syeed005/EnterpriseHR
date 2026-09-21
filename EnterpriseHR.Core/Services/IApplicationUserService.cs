using EnterpriseHR.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Services {
    public interface IApplicationUserService {
        Task<ApplicationUser> GetOrCreateCurrentUserAsync();
    }
}
