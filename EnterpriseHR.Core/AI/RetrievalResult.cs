using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.AI {
    public class RetrievalResult {
        public int DocumentChunkId { get; set; }
        public int DocumentId { get; set; }
        public int PageNumber { get; set; }
        public int ChunkIndex { get; set; }
        public string? SectionTitle { get; set; }
        public string Content { get; set; } = string.Empty;

        public int? SemanticRank { get; set; }
        public double? SemanticScore { get; set; }

        public int? FullTextRank { get; set; }
        public int? FullTextScore { get; set; }

        public double? RrfScore { get; set; }

        public bool IsExpandedContext { get; set; }
        public int? ExpandedFromChunkId { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Version { get; set; }
    }
}
