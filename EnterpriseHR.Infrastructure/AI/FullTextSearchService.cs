using EnterpriseHR.Core.AI;
using EnterpriseHR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.AI {
    public class FullTextSearchService : IFullTextSearchService {
        private readonly EnterpriseHrDbContext _dbContext;

        public FullTextSearchService(EnterpriseHrDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<FullTextSearchResult>> SearchAsync(string query, int topK = 5) {
            if (string.IsNullOrWhiteSpace(query))
                return [];

            var searchCondition = BuildSearchCondition(query);

            var sql = """
                SELECT TOP ({0})
                    c.DocumentChunkId,
                    p.DocumentId,
                    p.PageNumber,
                    c.ChunkIndex,
                    c.SectionTitle,
                    c.Content,
                    ft.[RANK] AS FullTextRank,
                    d.FileName,
                    d.Title,
                    d.Version
                FROM CONTAINSTABLE(DocumentChunks, (Content, SectionTitle), {1}) ft
                INNER JOIN DocumentChunks c
                    ON c.DocumentChunkId = ft.[KEY]
                INNER JOIN DocumentPages p
                    ON p.DocumentPageId = c.DocumentPageId
                INNER JOIN Documents d
                    ON d.DocumentId = p.DocumentId
                WHERE d.Status = 'Active'
                ORDER BY ft.[RANK] DESC;
                """;

            return await _dbContext.Database
                .SqlQueryRaw<FullTextSearchResult>(sql, topK, searchCondition)
                .ToListAsync();
        }

        private static string BuildSearchCondition(string query) {
            var terms = query
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => x.Length > 2)
                .Select(x => $"\"{x.Replace("\"", "\"\"")}\"");

            return string.Join(" OR ", terms);
        }
    }
}
