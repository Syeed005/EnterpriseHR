using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Documents {
    public interface ITextChunker {
        IReadOnlyList<TextChunk> Chunk(string text);
    }
}
