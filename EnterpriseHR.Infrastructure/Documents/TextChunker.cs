using EnterpriseHR.Core.Documents;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EnterpriseHR.Infrastructure.Documents {
    public class TextChunker : ITextChunker {
        private const int MaxChunkLength = 1200;

        public IReadOnlyList<TextChunk> Chunk(string text) {
            if (string.IsNullOrWhiteSpace(text))
                return [];

            var chunks = new List<TextChunk>();
            var sections = SplitIntoSections(text);

            foreach (var section in sections) {
                var sectionChunks = SplitContent(section.Content);

                foreach (var content in sectionChunks) {
                    var chunk = string.IsNullOrWhiteSpace(section.Heading)
                        ? content
                        : $"{section.Heading}{Environment.NewLine}{content}";

                    chunks.Add(new TextChunk {
                        SectionTitle = section.Heading,
                        Content = content
                    });
                }
            }

            return chunks;
        }
        private static void FlushCurrent(StringBuilder current, List<string> chunks) {
            if (current.Length == 0)
                return;

            chunks.Add(current.ToString().Trim());
            current.Clear();
        }

        private static IReadOnlyList<string> SplitContent(string text) {
            if (string.IsNullOrWhiteSpace(text))
                return [];

            var sentences = Regex.Split(text.Trim(), @"(?<=[.!?])\s+")
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var chunks = new List<string>();
            var current = new StringBuilder();

            foreach (var sentence in sentences) {
                if (current.Length > 0 && current.Length + sentence.Length + 1 > MaxChunkLength) {
                    chunks.Add(current.ToString().Trim());
                    current.Clear();
                }

                if (current.Length > 0)
                    current.Append(' ');

                current.Append(sentence);
            }

            if (current.Length > 0)
                chunks.Add(current.ToString().Trim());

            return chunks;
        }

        private static IEnumerable<string> SplitLargeParagraph(string paragraph) {
            var sentences = Regex.Split(paragraph, @"(?<=[.!?])\s+")
                .Where(x => !string.IsNullOrWhiteSpace(x));

            var current = new StringBuilder();

            foreach (var sentence in sentences) {
                if (current.Length > 0 && current.Length + sentence.Length + 1 > MaxChunkLength) {
                    yield return current.ToString().Trim();
                    current.Clear();
                }

                if (current.Length > 0)
                    current.Append(' ');

                current.Append(sentence);
            }

            if (current.Length > 0)
                yield return current.ToString().Trim();
        }

        public class TextSection {
            public string? Heading { get; set; }
            public string Content { get; set; } = string.Empty;            
        }

        private static IReadOnlyList<TextSection> SplitIntoSections(string text) {
            var headingPattern = @"(?<![:\d])\b(?:0[1-9]|[1-9]\d)\s+[A-Z][A-Za-z &'\/()\-]+";

            var matches = Regex.Matches(text, headingPattern);

            if (matches.Count == 0) {
                return
                [
                    new TextSection
            {
                Content = text.Trim()
            }
                ];
            }

            var sections = new List<TextSection>();

            // Preserve anything before the first numbered section.
            if (matches[0].Index > 0) {
                var introductoryContent = text[..matches[0].Index].Trim();

                if (!string.IsNullOrWhiteSpace(introductoryContent)) {
                    sections.Add(new TextSection {
                        Content = introductoryContent
                    });
                }
            }

            for (var i = 0; i < matches.Count; i++) {
                var match = matches[i];

                var start = match.Index;
                var end = i + 1 < matches.Count
                    ? matches[i + 1].Index
                    : text.Length;

                var sectionText = text[start..end].Trim();

                var heading = match.Value.Trim();

                var content = sectionText.Length > heading.Length
                    ? sectionText[heading.Length..].Trim()
                    : string.Empty;

                sections.Add(new TextSection {
                    Heading = heading,
                    Content = content
                });
            }

            return sections;
        }
    }
    
}
