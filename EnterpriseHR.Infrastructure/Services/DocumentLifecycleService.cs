using EnterpriseHR.Core.Audit;
using EnterpriseHR.Core.Services;
using EnterpriseHR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.Services {
    public class DocumentLifecycleService : IDocumentLifecycleService {
        private readonly EnterpriseHrDbContext _dbContext;
        private readonly IAuditService _auditService;

        public DocumentLifecycleService(EnterpriseHrDbContext dbContext, IAuditService auditService) {
            _dbContext = dbContext;
            _auditService = auditService;
        }

        public async Task ActivateAsync(int documentId) {
            // lifecycle implementation
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var document = await _dbContext.Documents
                .FirstOrDefaultAsync(d => d.DocumentId == documentId);

            if (document == null)
                throw new InvalidOperationException($"Document {documentId} was not found.");

            if (string.IsNullOrWhiteSpace(document.PolicyKey))
                throw new InvalidOperationException($"Document {documentId} does not have a PolicyKey.");

            if (document.Status == "Archived")
                throw new InvalidOperationException($"Archived document {documentId} cannot be activated.");

            if (document.Status == "Active")
                return;

            var existingActiveDocuments = await _dbContext.Documents
                .Where(d =>
                    d.DocumentId != documentId &&
                    d.PolicyKey == document.PolicyKey &&
                    d.Status == "Active")
                .ToListAsync();

            foreach (var activeDocument in existingActiveDocuments)
                activeDocument.Status = "Superseded";

            document.Status = "Active";

            await _dbContext.SaveChangesAsync();   
            await transaction.CommitAsync();

            await _auditService.RecordAsync(
                AuditActions.DocumentActivated,
                "Document",
                document.DocumentId.ToString(),
                $"PolicyKey={document.PolicyKey}");
        }
    }
}
