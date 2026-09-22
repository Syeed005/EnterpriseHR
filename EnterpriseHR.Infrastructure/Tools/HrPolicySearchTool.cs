using EnterpriseHR.Core.AI;
using EnterpriseHR.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.Tools {
    public class HrPolicySearchTool : IHrPolicySearchTool {
        private readonly IHybridSearchService _searchService;
        private readonly IContextExpansionService _contextExpansionService;

        public HrPolicySearchTool(IHybridSearchService searchService, IContextExpansionService contextExpansionService) {
            _searchService = searchService;
            _contextExpansionService = contextExpansionService;
        }

        public async Task<IReadOnlyList<HrPolicySearchResult>> SearchAsync(string query, int topK = 3) {
            var searchResults = await _searchService.SearchAsync(query, topK);
            var sources = await _contextExpansionService.ExpandAsync(searchResults);

            return sources.Select((source, index) => new HrPolicySearchResult {
                SourceNumber = index + 1,
                DocumentId = source.DocumentId,
                DocumentChunkId = source.DocumentChunkId,
                FileName = source.FileName,
                Title = source.Title,
                Version = source.Version,
                PageNumber = source.PageNumber,
                SectionTitle = source.SectionTitle,
                Content = source.Content
            }).ToList();
        }
    }
}
