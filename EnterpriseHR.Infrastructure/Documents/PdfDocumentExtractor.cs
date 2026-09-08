using EnterpriseHR.Core.Documents;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace EnterpriseHR.Infrastructure.Documents {
    public class PdfDocumentExtractor : IDocumentExtractor {
        public async Task<ExtractedDocument> ExtractAsync(string filePath) {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Document not found: {filePath}");

            var hash = await CalculateHashAsync(filePath);

            var result = new ExtractedDocument {
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                FileType = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant(),
                FileHash = hash
            };

            using var document = PdfDocument.Open(filePath);

            foreach (var page in document.GetPages()) {
                result.Pages.Add(new ExtractedPage {
                    PageNumber = page.Number,
                    Content = ContentOrderTextExtractor.GetText(page)
                });
            }

            return result;
        }

        private static async Task<string> CalculateHashAsync(string filePath) {
            await using var stream = File.OpenRead(filePath);
            var hash = await SHA256.HashDataAsync(stream);
            return Convert.ToHexString(hash);
        }
    }
}
