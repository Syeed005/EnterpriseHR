using EnterpriseHR.Core.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Services {
    public interface IToolCallingService {
        Task<ToolCallingResult> AskAsync(string question);
    }
}
