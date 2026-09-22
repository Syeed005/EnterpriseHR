using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Audit {
    public static class AuditActions {
        public const string DocumentIngested = "DocumentIngested";
        public const string DocumentActivated = "DocumentActivated";
        public const string DocumentSuperseded = "DocumentSuperseded";
        public const string DocumentArchived = "DocumentArchived";
    }
}
