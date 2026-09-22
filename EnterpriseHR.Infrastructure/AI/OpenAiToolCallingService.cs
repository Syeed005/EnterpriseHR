using EnterpriseHR.Core.Services;
using EnterpriseHR.Core.Tools;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EnterpriseHR.Infrastructure.AI {
    public class OpenAiToolCallingService : IToolCallingService {
        private readonly ChatClient _client;
        private readonly IEmployeeProfileTool _employeeProfileTool;

        private static readonly ChatTool GetMyEmployeeProfileTool = ChatTool.CreateFunctionTool(functionName: "get_my_employee_profile", functionDescription: "Get the authenticated employee's own profile, including employee number, department, office schedule, location, and employment status.");

        public OpenAiToolCallingService(string apiKey, IEmployeeProfileTool employeeProfileTool) {
            _client = new ChatClient("gpt-5-mini", apiKey);
            _employeeProfileTool = employeeProfileTool;
        }

        public async Task<ToolCallingResult> AskAsync(string question) {
            List<ChatMessage> messages = [
                new SystemChatMessage("You are an EnterpriseHR assistant. Use the available tool when the question requires information about the authenticated employee's own profile. Never invent employee-specific information."),
            new UserChatMessage(question)
            ];

            var options = new ChatCompletionOptions();
            options.Tools.Add(GetMyEmployeeProfileTool);

            var firstCompletion = (await _client.CompleteChatAsync(messages, options)).Value;

            if (firstCompletion.FinishReason != ChatFinishReason.ToolCalls) {
                return new ToolCallingResult {
                    Answer = firstCompletion.Content.Count > 0 ? firstCompletion.Content[0].Text : string.Empty,
                    ToolUsed = false
                };
            }

            messages.Add(new AssistantChatMessage(firstCompletion));

            string? usedToolName = null;

            foreach (var toolCall in firstCompletion.ToolCalls) {
                if (toolCall.FunctionName != "get_my_employee_profile")
                    throw new InvalidOperationException($"Unsupported tool '{toolCall.FunctionName}'.");

                usedToolName = toolCall.FunctionName;

                var profile = await _employeeProfileTool.GetMyEmployeeProfileAsync();
                var toolResult = profile is null ? JsonSerializer.Serialize(new { found = false }) : JsonSerializer.Serialize(new { found = true, profile.EmployeeNumber, profile.Department, profile.OfficeSchedule, profile.Location, profile.EmploymentStatus });

                messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
            }

            var finalCompletion = (await _client.CompleteChatAsync(messages, options)).Value;

            return new ToolCallingResult {
                Answer = finalCompletion.Content.Count > 0 ? finalCompletion.Content[0].Text : string.Empty,
                ToolUsed = true,
                ToolName = usedToolName
            };
        }
    }
}
