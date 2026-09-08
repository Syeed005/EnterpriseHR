using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Services {
    public interface IDocumentIngestionService {
        Task<int> IngestAsync(string filePath);
    }
}
