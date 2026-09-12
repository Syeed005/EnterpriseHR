using EnterpriseHR.Core.AI;
using EnterpriseHR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.AI {
    public class ContextExpansionService : IContextExpansionService {
        private readonly EnterpriseHrDbContext _dbContext;

        public ContextExpansionService(EnterpriseHrDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<RetrievalResult>> ExpandAsync(IReadOnlyList<RetrievalResult> results) {
            var expanded = new Dictionary<int, RetrievalResult>();

            foreach (var result in results) {
                expanded[result.DocumentChunkId] = result;

                if (string.IsNullOrWhiteSpace(result.SectionTitle))
                    continue;

                var sectionChunks = await _dbContext.DocumentChunks
                    .AsNoTracking()
                    .Where(c =>
                        c.DocumentPage.DocumentId == result.DocumentId &&
                        c.DocumentPage.PageNumber == result.PageNumber &&
                        c.SectionTitle == result.SectionTitle)
                    .OrderBy(c => c.ChunkIndex)
                    .Select(c => new RetrievalResult {
                        DocumentChunkId = c.DocumentChunkId,
                        DocumentId = c.DocumentPage.DocumentId,
                        PageNumber = c.DocumentPage.PageNumber,
                        ChunkIndex = c.ChunkIndex,
                        SectionTitle = c.SectionTitle,
                        Content = c.Content,
                        FileName = c.DocumentPage.Document.FileName,
                        Title = c.DocumentPage.Document.Title,
                        Version = c.DocumentPage.Document.Version,
                        IsExpandedContext = true,
                        ExpandedFromChunkId = result.DocumentChunkId
                    })
                    .ToListAsync();

                foreach (var chunk in sectionChunks) {
                    expanded.TryAdd(chunk.DocumentChunkId, chunk);
                }
            }

            return expanded.Values
                .OrderBy(x => x.DocumentId)
                .ThenBy(x => x.PageNumber)
                .ThenBy(x => x.ChunkIndex)
                .ToList();
        }
    }
}
