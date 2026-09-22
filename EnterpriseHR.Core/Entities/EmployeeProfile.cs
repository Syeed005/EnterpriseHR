using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Entities {
    public class EmployeeProfile {
        public int EmployeeProfileId { get; set; }
        public Guid ApplicationUserId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string OfficeSchedule { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string EmploymentStatus { get; set; } = string.Empty;

        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
