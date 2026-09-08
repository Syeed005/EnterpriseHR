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
        public DateTime CreatedAt { get; set; }
        public ICollection<DocumentPage> Pages { get; set; } = new List<DocumentPage>();
    }
}
