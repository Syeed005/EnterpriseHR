using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Tools {
    public interface IEmployeeProfileTool {
        Task<EmployeeProfileToolResult?> GetMyEmployeeProfileAsync();
    }
}
