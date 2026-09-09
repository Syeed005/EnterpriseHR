using EnterpriseHR.Core.Documents;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace EnterpriseHR.Infrastructure.Documents {
    public class DocumentMetadataExtractor : IDocumentMetadataExtractor {
        public DocumentMetadata Extract(ExtractedDocument document) {
            var text = string.Join(
                Environment.NewLine,
                document.Pages.Take(2).Select(x => x.Content));

            var metadata = new DocumentMetadata {
                Title = ExtractTitle(text, document.FileName),
                Version = ExtractValue(text, "VERSION", "Version"),
                EffectiveDate = ExtractDate(text, "EFFECTIVE DATE", "Effective Date"),
                IssuedBy = ExtractValue(text, "ISSUED BY", "Issued By", "Prepared by"),
                Audience = ExtractAudience(text),
                DocumentType = InferDocumentType(text, document.FileName)
            };

            return metadata;
        }

        private static string? ExtractValue(string text, params string[] labels) {
            foreach (var label in labels) {
                var pattern = $@"{Regex.Escape(label)}\s*[:\-]?\s*(.+)";
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

                if (match.Success) {
                    var value = match.Groups[1].Value.Trim();

                    if (!string.IsNullOrWhiteSpace(value))
                        return GetFirstReasonableValue(value);
                }
            }

            return null;
        }

        private static DateOnly? ExtractDate(string text, params string[] labels) {
            var value = ExtractValue(text, labels);

            if (string.IsNullOrWhiteSpace(value))
                return null;

            var match = Regex.Match(value, @"\d{1,2}\s+[A-Za-z]+\s+\d{4}");

            if (!match.Success)
                return null;

            if (DateOnly.TryParse(match.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;

            return null;
        }

        private static string? ExtractAudience(string text) {
            if (text.Contains("Applies to all BJIT employees", StringComparison.OrdinalIgnoreCase))
                return "AllEmployees";

            return null;
        }

        private static string? InferDocumentType(string text, string fileName) {
            if (text.Contains("EMPLOYEE GUIDELINE", StringComparison.OrdinalIgnoreCase))
                return "EmployeeGuideline";

            if (text.Contains("POLICY", StringComparison.OrdinalIgnoreCase) ||
                fileName.Contains("Policy", StringComparison.OrdinalIgnoreCase))
                return "Policy";

            return null;
        }

        private static string ExtractTitle(string text, string fileName) {
            var policyTitle = ExtractValue(text, "Policy Title");

            if (!string.IsNullOrWhiteSpace(policyTitle))
                return policyTitle;

            return Path.GetFileNameWithoutExtension(fileName);
        }

        private static string GetFirstReasonableValue(string value) {
            var separators = new[]
            {
            "VERSION",
            "ISSUED BY",
            "EFFECTIVE DATE",
            "PREPARED BY",
            "APPROVED BY",
            "DATE OF ISSUE"
        };

            foreach (var separator in separators) {
                var index = value.IndexOf(separator, StringComparison.OrdinalIgnoreCase);

                if (index > 0)
                    return value[..index].Trim();
            }

            return value.Trim();
        }
    }
}
