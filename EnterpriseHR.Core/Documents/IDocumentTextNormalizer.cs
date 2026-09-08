using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Documents {
    public interface IDocumentTextNormalizer {
        string Normalize(string text);
    }
}
