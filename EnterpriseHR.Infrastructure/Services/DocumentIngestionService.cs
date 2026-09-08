using EnterpriseHR.Core.Documents;
using EnterpriseHR.Core.Entities;
using EnterpriseHR.Core.Services;
using EnterpriseHR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.Services {
    public class DocumentIngestionService : IDocumentIngestionService {
        private readonly EnterpriseHrDbContext _db;
        private readonly IDocumentExtractor _extractor;
        private readonly IDocumentTextNormalizer _normalizer;

        public DocumentIngestionService(EnterpriseHrDbContext db, IDocumentExtractor extractor, IDocumentTextNormalizer normalizer) {
            _db = db;
            _extractor = extractor;
            _normalizer = normalizer;
        }

        public async Task<int> IngestAsync(string filePath) {
            var extracted = await _extractor.ExtractAsync(filePath);

            var exists = await _db.Documents.AnyAsync(x => x.FileHash == extracted.FileHash);

            if (exists)
                throw new InvalidOperationException("This document has already been ingested.");

            var document = new Document {
                FileName = extracted.FileName,
                FilePath = extracted.FilePath,
                FileType = extracted.FileType,
                FileHash = extracted.FileHash,
                Title = Path.GetFileNameWithoutExtension(extracted.FileName),
                CreatedAt = DateTime.UtcNow
            };

            foreach (var page in extracted.Pages) {
                document.Pages.Add(new DocumentPage {
                    PageNumber = page.PageNumber,
                    RawContent = page.Content,
                    NormalizedContent = _normalizer.Normalize(page.Content)
                });
            }

            _db.Documents.Add(document);
            await _db.SaveChangesAsync();

            return document.DocumentId;
        }
    }
}
