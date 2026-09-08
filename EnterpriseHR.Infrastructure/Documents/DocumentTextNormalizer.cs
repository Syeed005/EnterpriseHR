using EnterpriseHR.Core.Documents;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EnterpriseHR.Infrastructure.Documents {
    public class DocumentTextNormalizer : IDocumentTextNormalizer {
        public string Normalize(string text) {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var normalized = text;

            normalized = Regex.Replace(normalized, @"https?://\S+", " ");

            normalized = Regex.Replace(
                normalized,
                @"\d{4}/\d{2}/\d{2},\s*\d{1,2}:\d{2}",
                " ");

            normalized = Regex.Replace(
                normalized,
                @"Page\s+\d+\s+of\s+\d+",
                " ",
                RegexOptions.IgnoreCase);

            normalized = normalized.Replace("o!ce", "office");

            normalized = Regex.Replace(normalized, @"[ \t]+", " ");
            normalized = Regex.Replace(normalized, @"\s*\r?\n\s*", Environment.NewLine);

            return normalized.Trim();
        }
    }
}
