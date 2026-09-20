using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Services {
    public interface IDocumentLifecycleService {
        Task ActivateAsync(int documentId);
    }
}
