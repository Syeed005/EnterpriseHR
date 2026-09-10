using EnterpriseHR.Core.Documents;
using EnterpriseHR.Core.Entities;
using EnterpriseHR.Core.Services;
using EnterpriseHR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.Services {
    public class DocumentChunkingService : IDocumentChunkingService {
        private readonly EnterpriseHrDbContext _db;
        private readonly ITextChunker _chunker;

        public DocumentChunkingService(EnterpriseHrDbContext db, ITextChunker chunker) {
            _db = db;
            _chunker = chunker;
        }
        public async Task<int> ChunkDocumentAsync(int documentId) {
            var pages = await _db.DocumentPages
                .Where(x => x.DocumentId == documentId)
                .Include(x => x.Chunks)
                .OrderBy(x => x.PageNumber)
                .ToListAsync();

            if (pages.Count == 0)
                throw new InvalidOperationException($"Document {documentId} was not found or has no pages.");

            var created = 0;

            foreach (var page in pages) {
                if (page.Chunks.Count > 0)
                    _db.DocumentChunks.RemoveRange(page.Chunks);

                var chunks = _chunker.Chunk(page.NormalizedContent);

                for (var i = 0; i < chunks.Count; i++) {
                    page.Chunks.Add(new DocumentChunk {
                        ChunkIndex = i,
                        SectionTitle = chunks[i].SectionTitle,
                        Content = chunks[i].Content,
                        CharacterCount = chunks[i].Content.Length
                    });

                    created++;
                }
            }

            await _db.SaveChangesAsync();

            return created;
        }

    }
}
