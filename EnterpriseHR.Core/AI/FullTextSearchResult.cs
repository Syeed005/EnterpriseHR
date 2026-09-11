using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.AI {
    public class FullTextSearchResult {
        public int DocumentChunkId { get; set; }
        public int DocumentId { get; set; }
        public int PageNumber { get; set; }
        public int ChunkIndex { get; set; }
        public string? SectionTitle { get; set; }
        public string Content { get; set; } = string.Empty;

        public int FullTextRank { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Version { get; set; }
    }
}
