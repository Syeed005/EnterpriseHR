using EnterpriseHR.Core.Requests;

namespace EnterpriseHR.Api.Validation {
    public static class RequestValidator {
        public static void Validate(AskHrAssistantRequest request) {
            if (request.SessionId == Guid.Empty)
                throw new ArgumentException("A valid session ID is required.");

            if (string.IsNullOrWhiteSpace(request.Question))
                throw new ArgumentException("Question is required.");

            if (request.Question.Length > 2000)
                throw new ArgumentException("Question cannot exceed 2000 characters.");
        }
    }
}
