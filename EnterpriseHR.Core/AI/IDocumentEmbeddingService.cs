using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.AI {
    public interface IDocumentEmbeddingService {
        Task<int> GenerateEmbeddingsAsync(int documentId);
    }
}
