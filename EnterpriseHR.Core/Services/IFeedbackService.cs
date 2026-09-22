using EnterpriseHR.Core.Entities;
using EnterpriseHR.Core.Feedback;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Services {
    public interface IFeedbackService {
        Task<AnswerFeedback> SubmitAsync(SubmitFeedbackRequest request);
    }
}
