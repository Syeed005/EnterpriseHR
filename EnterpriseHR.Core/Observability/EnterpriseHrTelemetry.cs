using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EnterpriseHR.Core.Observability {
    public class EnterpriseHrTelemetry {
        public const string ActivitySourceName = "EnterpriseHR";

        public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    }
}
