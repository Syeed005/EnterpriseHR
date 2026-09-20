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

        public async Task<ChatGenerationResult> GenerateAnswerAsync(string prompt) {
            var completion = await _client.CompleteChatAsync(prompt);
            var result = completion.Value;

            return new ChatGenerationResult {
                Content = result.Content[0].Text,
                InputTokens = result.Usage.InputTokenCount,
                OutputTokens = result.Usage.OutputTokenCount,
                TotalTokens = result.Usage.TotalTokenCount,
                Provider = "OpenAI",
                Model = "gpt-5-mini"
            };
        }
    }
}
