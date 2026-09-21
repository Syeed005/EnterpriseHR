using EnterpriseHR.Core.Entities;
using EnterpriseHR.Core.Services;
using EnterpriseHR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.Services {
    public class ConversationService : IConversationService {
        private readonly EnterpriseHrDbContext _dbContext;
        private readonly IApplicationUserService _applicationUserService;

        public ConversationService(EnterpriseHrDbContext dbContext, IApplicationUserService applicationUserService) {
            _dbContext = dbContext;
            _applicationUserService = applicationUserService;
        }

        public async Task<ChatSession> CreateSessionAsync() {
            var user = await _applicationUserService.GetOrCreateCurrentUserAsync();
            var now = DateTime.UtcNow;

            var session = new ChatSession {
                ChatSessionId = Guid.NewGuid(),
                ApplicationUserId = user.ApplicationUserId,
                CreatedAtUtc = now,
                LastActivityAtUtc = now
            };

            _dbContext.ChatSessions.Add(session);
            await _dbContext.SaveChangesAsync();

            return session;
        }

        public async Task<ChatMessage> AddMessageAsync(Guid sessionId, string role, string content) {
            var user = await _applicationUserService.GetOrCreateCurrentUserAsync();
            var session = await _dbContext.ChatSessions
                .SingleOrDefaultAsync(x =>x.ChatSessionId == sessionId &&
                                          x.ApplicationUserId == user.ApplicationUserId);

            if (session == null)
                throw new InvalidOperationException($"Chat session '{sessionId}' was not found.");

            var now = DateTime.UtcNow;

            var message = new ChatMessage {
                ChatSessionId = sessionId,
                Role = role,
                Content = content,
                CreatedAtUtc = now
            };

            session.LastActivityAtUtc = now;

            _dbContext.ChatMessages.Add(message);
            await _dbContext.SaveChangesAsync();

            return message;
        }

        public async Task<IReadOnlyList<ChatMessage>> GetRecentMessagesAsync(Guid sessionId, int count = 6) {
            var user = await _applicationUserService.GetOrCreateCurrentUserAsync();
            var ownsSession = await _dbContext.ChatSessions
                                .AnyAsync(x => x.ChatSessionId == sessionId &&
                                               x.ApplicationUserId == user.ApplicationUserId);
            if (!ownsSession)
                throw new UnauthorizedAccessException("The chat session was not found or is not accessible.");

            var messages = await _dbContext.ChatMessages
                .AsNoTracking()
                .Where(x => x.ChatSessionId == sessionId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(count)
                .ToListAsync();

            return messages
                .OrderBy(x => x.CreatedAtUtc)
                .ToList();
        }
    }
}
