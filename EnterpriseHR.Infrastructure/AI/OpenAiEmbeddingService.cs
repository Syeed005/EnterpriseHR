using EnterpriseHR.Core.AI;
using OpenAI.Embeddings;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.AI {
    public class OpenAiEmbeddingService : IEmbeddingService {
        private readonly EmbeddingClient _client;

        public OpenAiEmbeddingService(string apiKey) {
            _client = new EmbeddingClient("text-embedding-3-small", apiKey);
        }
        public async Task<float[]> GenerateEmbeddingAsync(string text) {
            // Call OpenAI embeddings API
            // Return vector as float[]
            var embedding = await _client.GenerateEmbeddingAsync(text);
            return embedding.Value.ToFloats().ToArray();
        }
    }
}
