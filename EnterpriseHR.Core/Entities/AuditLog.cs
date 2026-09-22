using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Entities {
    public class AuditLog {
        public long AuditLogId { get; set; }
        public Guid ApplicationUserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
        public string ResourceId { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
