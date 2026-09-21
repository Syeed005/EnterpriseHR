using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Entities {
    public class AiUsageRecord {
        public int AiUsageRecordId { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public string Provider { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public int InputTokens { get; set; }

        public int OutputTokens { get; set; }

        public int TotalTokens { get; set; }

        public decimal InputCostUsd { get; set; }

        public decimal OutputCostUsd { get; set; }

        public decimal TotalCostUsd { get; set; }

        public string Operation { get; set; } = string.Empty;

        public string? TraceId { get; set; }
    }
}
