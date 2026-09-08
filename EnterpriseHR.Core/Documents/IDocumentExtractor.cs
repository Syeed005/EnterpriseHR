using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Documents {
    public interface IDocumentExtractor {
        Task<ExtractedDocument> ExtractAsync(string filePath);
    }
}
