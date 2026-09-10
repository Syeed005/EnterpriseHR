using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Services {
    public interface IDocumentChunkingService {
        Task<int> ChunkDocumentAsync(int documentId);
    }
}
