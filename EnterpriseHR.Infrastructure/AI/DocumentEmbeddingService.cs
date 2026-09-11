using EnterpriseHR.Core.AI;
using EnterpriseHR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EnterpriseHR.Infrastructure.AI {
    public class DocumentEmbeddingService : IDocumentEmbeddingService {
        private readonly EnterpriseHrDbContext _dbContext;
        private readonly IEmbeddingService _embeddingService;

        public DocumentEmbeddingService(EnterpriseHrDbContext dbContext, IEmbeddingService embeddingService) {
            _dbContext = dbContext;
            _embeddingService = embeddingService;
        }

        public async Task<int> GenerateEmbeddingsAsync(int documentId) {
            var chunks = await _dbContext.DocumentChunks
                .Where(x => x.DocumentPage.DocumentId == documentId && string.IsNullOrWhiteSpace(x.EmbeddingJson))
                .OrderBy(x => x.DocumentPage.PageNumber)
                .ThenBy(x => x.ChunkIndex)
                .ToListAsync();

            foreach (var chunk in chunks) {
                var embedding = await _embeddingService.GenerateEmbeddingAsync(chunk.Content);
                chunk.EmbeddingJson = JsonSerializer.Serialize(embedding);
            }

            await _dbContext.SaveChangesAsync();

            return chunks.Count;
        }
    }
}
