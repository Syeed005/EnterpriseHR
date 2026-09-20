using EnterpriseHR.Core.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.AI {
    public class AiCostCalculator : IAiCostCalculator{
        private const decimal Gpt5MiniInputPerMillion = 0.25m;
        private const decimal Gpt5MiniOutputPerMillion = 2.00m;

        public AiCostResult Calculate(string model, int inputTokens, int outputTokens) {
            if (!model.Equals("gpt-5-mini", StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException($"Cost calculation is not configured for model '{model}'.");

            var inputCost = inputTokens / 1_000_000m * Gpt5MiniInputPerMillion;
            var outputCost = outputTokens / 1_000_000m * Gpt5MiniOutputPerMillion;

            return new AiCostResult {
                InputCostUsd = inputCost,
                OutputCostUsd = outputCost,
                TotalCostUsd = inputCost + outputCost
            };
        }
    }
}
