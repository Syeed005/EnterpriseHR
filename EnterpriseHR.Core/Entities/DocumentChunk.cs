using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Entities {
    public class DocumentChunk {
        public int DocumentChunkId { get; set; }
        public int DocumentPageId { get; set; }
        public int ChunkIndex { get; set; }
        public string? SectionTitle { get; set; }
        public string Content { get; set; } = string.Empty;
        public int CharacterCount { get; set; }
        public DocumentPage DocumentPage { get; set; } = null!;
    }
}
