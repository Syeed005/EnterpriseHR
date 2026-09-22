using EnterpriseHR.Core.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Entities {
    public class Document {
        public int DocumentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string? FileHash { get; set; }
        public string? Title { get; set; }
        public string? DocumentType { get; set; }
        public string? Version { get; set; }
        public DateOnly? EffectiveDate { get; set; }
        public string? IssuedBy { get; set; }
        public string? Audience { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? PolicyKey { get; set; }
        public string AccessLevel { get; set; } = DocumentAccessLevels.Employee;
        public ICollection<DocumentPage> Pages { get; set; } = new List<DocumentPage>();
    }
}
