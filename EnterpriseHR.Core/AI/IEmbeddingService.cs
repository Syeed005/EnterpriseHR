using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.AI {
    public interface IEmbeddingService {
        Task<float[]> GenerateEmbeddingAsync(string text);
    }
}
