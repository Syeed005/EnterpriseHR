using EnterpriseHR.Core.Entities;
using EnterpriseHR.Core.Feedback;
using EnterpriseHR.Core.Services;
using EnterpriseHR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.Services {
    public class FeedbackService : IFeedbackService {
        private readonly EnterpriseHrDbContext _dbContext;
        private readonly IApplicationUserService _applicationUserService;

        public FeedbackService(EnterpriseHrDbContext dbContext, IApplicationUserService applicationUserService) {
            _dbContext = dbContext;
            _applicationUserService = applicationUserService;
        }

        public async Task<AnswerFeedback> SubmitAsync(SubmitFeedbackRequest request) {
            var user = await _applicationUserService.GetOrCreateCurrentUserAsync();

            var message = await _dbContext.ChatMessages
                .AsNoTracking()
                .Where(m =>
                    m.ChatMessageId == request.ChatMessageId &&
                    m.ChatSession.ApplicationUserId == user.ApplicationUserId)
                .Select(m => new {
                    m.ChatMessageId,
                    m.Role
                })
                .SingleOrDefaultAsync();

            if (message is null)
                throw new UnauthorizedAccessException("The chat message was not found or is not accessible.");

            if (!string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Feedback can only be submitted for assistant messages.");

            var existingFeedback = await _dbContext.AnswerFeedback
                .SingleOrDefaultAsync(x =>
                    x.ChatMessageId == request.ChatMessageId &&
                    x.ApplicationUserId == user.ApplicationUserId);

            if (existingFeedback is not null) {
                existingFeedback.IsHelpful = request.IsHelpful;
                existingFeedback.Comment = request.Comment;

                await _dbContext.SaveChangesAsync();
                return existingFeedback;
            }

            var feedback = new AnswerFeedback {
                ChatMessageId = request.ChatMessageId,
                ApplicationUserId = user.ApplicationUserId,
                IsHelpful = request.IsHelpful,
                Comment = request.Comment,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.AnswerFeedback.Add(feedback);
            await _dbContext.SaveChangesAsync();

            return feedback;
        }
    }
}
