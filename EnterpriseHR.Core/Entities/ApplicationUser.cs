using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Entities {
    public class ApplicationUser {
        public Guid ApplicationUserId { get; set; }
        public string ExternalUserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee";
        public DateTime CreatedAtUtc { get; set; }

        public ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
    }
}
