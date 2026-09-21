using EnterpriseHR.Core.AI;
using OpenAI.Chat;
using EnterpriseHR.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.AI {
    public class OpenAiQuestionContextualizer : IQuestionContextualizer {
        private readonly ChatClient _client;

        public OpenAiQuestionContextualizer(string apiKey) {
            _client = new ChatClient("gpt-5-mini", apiKey);
        }

        public async Task<string> ContextualizeAsync(string question, IReadOnlyList<EnterpriseHR.Core.Entities.ChatMessage> history) {
            if (history.Count == 0)
                return question;

            var historyText = new StringBuilder();

            foreach (var message in history)
                historyText.AppendLine($"{message.Role}: {message.Content}");

            var prompt = $"""
                Rewrite the current user question as a standalone search query using the conversation history when necessary.

                Rules:
                - Do not answer the question.
                - Preserve the user's intent.
                - Resolve references such as "it", "they", "that", "there", or "what about Friday".
                - Use conversation history only to clarify the current question.
                - Do not add facts that are not present in the question or conversation.
                - Return only the rewritten query.

                Conversation history:
                {historyText}

                Current question:
                {question}
                """;

            var completion = await _client.CompleteChatAsync(prompt);

            return completion.Value.Content[0].Text.Trim();
        }
    }
}
