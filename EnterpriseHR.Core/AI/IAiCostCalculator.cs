using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.AI {
    public interface IAiCostCalculator {
        AiCostResult Calculate(string model, int inputTokens, int outputTokens);
    }
}
