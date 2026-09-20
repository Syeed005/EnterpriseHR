using EnterpriseHR.Core.AI;
using EnterpriseHR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EnterpriseHR.Infrastructure.AI {
    public class SemanticSearchService : ISemanticSearchService {
        private readonly EnterpriseHrDbContext _dbContext;
        private readonly IEmbeddingService _embeddingService;

        public SemanticSearchService(EnterpriseHrDbContext dbContext, IEmbeddingService embeddingService) {
            _dbContext = dbContext;
            _embeddingService = embeddingService;
        }

        public async Task<IReadOnlyList<SemanticSearchResult>> SearchAsync(string query, int topK = 3) {
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);

            var chunks = await _dbContext.DocumentChunks
                .AsNoTracking()
                .Where(x => x.EmbeddingJson != null
                      && x.EmbeddingJson != ""
                      && x.DocumentPage.Document.Status == "Active"
                ).Select(x => new {
                    x.DocumentChunkId,
                    x.ChunkIndex,
                    x.SectionTitle,
                    x.Content,
                    x.EmbeddingJson,
                    x.DocumentPage.DocumentId,
                    x.DocumentPage.PageNumber,
                    x.DocumentPage.Document.FileName,
                    x.DocumentPage.Document.Title,
                    x.DocumentPage.Document.Version
                })
                .ToListAsync();

            var results = new List<SemanticSearchResult>();

            foreach (var chunk in chunks) {
                var chunkEmbedding = JsonSerializer.Deserialize<float[]>(chunk.EmbeddingJson!);

                if (chunkEmbedding == null)
                    continue;

                var score = CosineSimilarity(queryEmbedding, chunkEmbedding);

                results.Add(new SemanticSearchResult {
                    DocumentChunkId = chunk.DocumentChunkId,
                    DocumentId = chunk.DocumentId,
                    FileName = chunk.FileName,
                    Title = chunk.Title,
                    Version = chunk.Version,
                    PageNumber = chunk.PageNumber,
                    ChunkIndex = chunk.ChunkIndex,
                    SectionTitle = chunk.SectionTitle,
                    Content = chunk.Content,
                    RetrievalScore = score,
                    IsExpandedContext = false,
                    ExpandedFromChunkId = null
                });
            }

            return results
                .OrderByDescending(x => x.RetrievalScore)
                .Take(topK)
                .ToList();
        }


        private static double CosineSimilarity(float[] a, float[] b) {
            if (a.Length != b.Length)
                throw new InvalidOperationException("Embedding dimensions do not match.");

            double dotProduct = 0;
            double magnitudeA = 0;
            double magnitudeB = 0;

            for (var i = 0; i < a.Length; i++) {
                dotProduct += a[i] * b[i];
                magnitudeA += a[i] * a[i];
                magnitudeB += b[i] * b[i];
            }

            if (magnitudeA == 0 || magnitudeB == 0)
                return 0;

            return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
        }
    }
}

