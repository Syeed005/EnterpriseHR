using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.AI {
    public class SemanticSearchResult {
        public int DocumentChunkId { get; set; }
        public int DocumentId { get; set; }
        public int PageNumber { get; set; }
        public int ChunkIndex { get; set; }
        public string? SectionTitle { get; set; }
        public string Content { get; set; } = string.Empty;

        public double? RetrievalScore { get; set; }
        public bool IsExpandedContext { get; set; }
        public int? ExpandedFromChunkId { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Version { get; set; }
    }
}
