using EnterpriseHR.Core.Audit;
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
        private readonly IDocumentMetadataExtractor _metadataExtractor;
        private readonly IAuditService _auditService;

        public DocumentIngestionService(EnterpriseHrDbContext db, IDocumentExtractor extractor, IDocumentTextNormalizer normalizer, IDocumentMetadataExtractor metadataExtractor, IAuditService auditService) {
            _db = db;
            _extractor = extractor;
            _normalizer = normalizer;
            _metadataExtractor = metadataExtractor;
            _auditService = auditService;
        }

        public async Task<int> IngestAsync(string filePath) {
            var extracted = await _extractor.ExtractAsync(filePath);
            var metadata = _metadataExtractor.Extract(extracted);

            var exists = await _db.Documents.AnyAsync(x => x.FileHash == extracted.FileHash);

            if (exists)
                throw new InvalidOperationException("This document has already been ingested.");

            var document = new Document {
                FileName = extracted.FileName,
                FilePath = extracted.FilePath,
                FileType = extracted.FileType,
                FileHash = extracted.FileHash,

                Title = metadata.Title ?? Path.GetFileNameWithoutExtension(extracted.FileName),
                DocumentType = metadata.DocumentType,
                Version = metadata.Version,
                EffectiveDate = metadata.EffectiveDate,
                IssuedBy = metadata.IssuedBy,
                Audience = metadata.Audience,

                Status = "Pending",
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
            await _auditService.RecordAsync(
                AuditActions.DocumentIngested,
                "Document",
                document.DocumentId.ToString(),
                $"FileName={document.FileName}");
            return document.DocumentId;
        }
    }
}
