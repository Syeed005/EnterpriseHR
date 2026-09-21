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

        public ConversationService(EnterpriseHrDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task<ChatSession> CreateSessionAsync() {
            var now = DateTime.UtcNow;

            var session = new ChatSession {
                ChatSessionId = Guid.NewGuid(),
                CreatedAtUtc = now,
                LastActivityAtUtc = now
            };

            _dbContext.ChatSessions.Add(session);
            await _dbContext.SaveChangesAsync();

            return session;
        }

        public async Task<ChatMessage> AddMessageAsync(Guid sessionId, string role, string content) {
            var session = await _dbContext.ChatSessions.FirstOrDefaultAsync(x => x.ChatSessionId == sessionId);

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
            return await _dbContext.ChatMessages
                .AsNoTracking()
                .Where(x => x.ChatSessionId == sessionId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(count)
                .OrderBy(x => x.CreatedAtUtc)
                .ToListAsync();
        }
    }
}
