using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Documents {
    public class DocumentMetadata {
        public string? Title { get; set; }
        public string? DocumentType { get; set; }
        public string? Version { get; set; }
        public DateOnly? EffectiveDate { get; set; }
        public string? IssuedBy { get; set; }
        public string? Audience { get; set; }
    }
}
