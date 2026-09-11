using EnterpriseHR.Core.AI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.AI {
    public class OpenAiChatService : IChatService {
        private readonly ChatClient _client;

        public OpenAiChatService(string apiKey) {
            _client = new ChatClient("gpt-5-mini", apiKey);
        }

        public async Task<string> GenerateAnswerAsync(string prompt) {
            var completion = await _client.CompleteChatAsync(prompt);

            return completion.Value.Content[0].Text;
        }
    }
}
