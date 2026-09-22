using EnterpriseHR.Core.Services;
using EnterpriseHR.Core.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.Tools {
    public class EmployeeProfileTool : IEmployeeProfileTool {
        private readonly IEmployeeProfileService _employeeProfileService;

        public EmployeeProfileTool(IEmployeeProfileService employeeProfileService) {
            _employeeProfileService = employeeProfileService;
        }

        public async Task<EmployeeProfileToolResult?> GetMyEmployeeProfileAsync() {
            var profile = await _employeeProfileService.GetMyProfileAsync();

            if (profile is null)
                return null;

            return new EmployeeProfileToolResult {
                EmployeeNumber = profile.EmployeeNumber,
                Department = profile.Department,
                OfficeSchedule = profile.OfficeSchedule,
                Location = profile.Location,
                EmploymentStatus = profile.EmploymentStatus
            };
        }
    }
}
