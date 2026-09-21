using EnterpriseHR.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Core.Services {
    public interface IConversationService {
        Task<ChatSession> CreateSessionAsync();
        Task<ChatMessage> AddMessageAsync(Guid sessionId, string role, string content);
        Task<IReadOnlyList<ChatMessage>> GetRecentMessagesAsync(Guid sessionId, int count = 6);
    }
}
