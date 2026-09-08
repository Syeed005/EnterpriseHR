using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Entities {
    public class DocumentPage {
        public int DocumentPageId { get; set; }
        public int DocumentId { get; set; }
        public int PageNumber { get; set; }
        public string Content { get; set; } = string.Empty;
        public Document Document { get; set; } = null!;
    }
}
