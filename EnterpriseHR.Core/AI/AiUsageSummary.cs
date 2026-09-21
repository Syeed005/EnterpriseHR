using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.AI {
    public class AiUsageSummary {
        public int RequestCount { get; set; }
        public long TotalInputTokens { get; set; }
        public long TotalOutputTokens { get; set; }
        public long TotalTokens { get; set; }
        public decimal TotalCostUsd { get; set; }
        public decimal AverageCostPerRequestUsd { get; set; }
    }
}
