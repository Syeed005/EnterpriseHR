using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Documents {
    public interface IDocumentMetadataExtractor {
        DocumentMetadata Extract(ExtractedDocument document);
    }
}
