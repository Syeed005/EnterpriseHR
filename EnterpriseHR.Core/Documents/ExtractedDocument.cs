using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Documents {
    public class ExtractedDocument {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string FileHash { get; set; } = string.Empty;
        public List<ExtractedPage> Pages { get; set; } = [];
    }
}
