using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Exceptions {
    public class AiServiceException : Exception {
        public AiServiceException(string message, Exception? innerException = null) : base(message, innerException) { }
    }
}
