using EnterpriseHR.Core.AI;
using EnterpriseHR.Core.Exceptions;
using EnterpriseHR.Core.Models;
using EnterpriseHR.Core.Services;
using EnterpriseHR.Core.Tools;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EnterpriseHR.Infrastructure.AI {
    public class HrAssistantService : IHrAssistantService {
        private readonly ChatClient _client;
        private readonly decimal _inputCostPerMillionTokens;
        private readonly decimal _outputCostPerMillionTokens;
        private readonly IEmployeeProfileTool _employeeProfileTool;
        private readonly IHrPolicySearchTool _hrPolicySearchTool;
        private readonly IConversationService _conversationService;

        public HrAssistantService(string apiKey, decimal inputCostPerMillionTokens, decimal outputCostPerMillionTokens, IEmployeeProfileTool employeeProfileTool, IHrPolicySearchTool hrPolicySearchTool, IConversationService conversationService) {
            _client = new ChatClient("gpt-5-mini", apiKey);
            _inputCostPerMillionTokens = inputCostPerMillionTokens;
            _outputCostPerMillionTokens = outputCostPerMillionTokens;
            _employeeProfileTool = employeeProfileTool;
            _hrPolicySearchTool = hrPolicySearchTool;
            _conversationService = conversationService;
        }

        private static readonly ChatTool GetMyEmployeeProfileTool = ChatTool.CreateFunctionTool(functionName: "get_my_employee_profile", functionDescription: "Get structured information about the current authenticated employee, including employee number, department, office schedule, location, and employment status.");

        private static readonly ChatTool SearchHrPolicyTool = ChatTool.CreateFunctionTool(functionName: "search_hr_policy", functionDescription: "Search authorized authoritative HR policy documents for information needed to answer the employee's question.", functionParameters: BinaryData.FromString("""
              {
                "type": "object",
                "properties": {
                  "query": {
                    "type": "string",
                    "description": "A focused search query describing the HR policy information needed."
                  }
                },
                "required": ["query"],
                "additionalProperties": false
              }
        """));

        public async Task<HrAssistantResult> AskAsync(Guid sessionId, string question, CancellationToken cancellationToken = default) {
            
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(30));
            var operationToken = timeoutCts.Token;

            var history = await _conversationService.GetRecentMessagesAsync(sessionId, 6);
            await _conversationService.AddMessageAsync(sessionId, "user", question);

            var historyText = history.Count == 0 ? "No previous conversation." : string.Join("\n", history.Select(x => $"{x.Role}: {x.Content}"));

            List<ChatMessage> messages = [
                new SystemChatMessage($"""
                You are an internal EnterpriseHR assistant.

                You have approved tools for retrieving authoritative information.

                - Use get_my_employee_profile when the question requires facts about the current authenticated employee.
                - Use search_hr_policy when the question requires HR policy, rules, schedules, eligibility, procedures, or other authoritative HR document    information.
                - Use both tools when answering requires both employee-specific data and HR policy information.
                - Do not offer to perform another policy search after the policy search tool reports that the search limit has been reached.

                - Never claim or imply that you can contact HR, retrieve HR contact information, send messages, create tickets, or perform any action unless an available tool explicitly provides that capability.

                - If the available authorized information does not contain the requested information, clearly state that it was not found in the HR information available to the current user. Do not invent an alternative capability.
                - If search_hr_policy does not return information sufficient to answer the question, do not repeatedly search for the same unavailable information.
                - After a reasonable search attempt, state that the available authorized HR information does not provide the requested information.
                - Never infer, guess, or reconstruct information that is absent from the authorized tool results.
                - Never claim or offer capabilities that are not provided by the available tools.
                - Do not suggest that get_my_employee_profile can retrieve fields that its tool description does not provide.
                - If authorized policy search results do not contain the requested information, simply state that the available authorized HR documents do not provide that information.
                - Do not cite unrelated search results as evidence for information they do not support.
                - Whenever you use factual information returned by search_hr_policy, cite the supporting result using its SourceNumber in the answer, for example [Source 1].
                - Never invent employee-specific information or HR policy.
                - Do not treat previous assistant messages as authoritative evidence.
                - You may call another approved tool if information returned by the first tool shows that additional authoritative information is required.

                Previous conversation:
                {historyText}

                When sufficient authoritative information is available, answer the employee's question clearly and concisely.
                """),
                new UserChatMessage(question)
            ];

            var options = new ChatCompletionOptions();
            options.Tools.Add(GetMyEmployeeProfileTool);
            options.Tools.Add(SearchHrPolicyTool);

            var toolsUsed = new List<string>();
            var policySources = new List<HrPolicySearchResult>();
            var policySearchCount = 0;
            var totalInputTokens = 0;
            var totalOutputTokens = 0;



            // Agent loop will go here.

            const int maxRounds = 4;

            for (var round = 0; round < maxRounds; round++) {
                operationToken.ThrowIfCancellationRequested();
                ChatCompletion completion;

                try {
                    completion = (await _client.CompleteChatAsync(messages, options, operationToken)).Value;
                } catch (OperationCanceledException) {
                    throw;
                } catch (Exception ex) {
                    throw new AiServiceException("The AI provider request failed.", ex);
                }

                totalInputTokens += completion.Usage.InputTokenCount;
                totalOutputTokens += completion.Usage.OutputTokenCount;

                if (completion.FinishReason != ChatFinishReason.ToolCalls) {
                    var answer = completion.Content.Count > 0 ? completion.Content[0].Text : string.Empty;
                    var assistantMessage = await _conversationService.AddMessageAsync(sessionId, "assistant", answer);
                    var totalTokens = totalInputTokens + totalOutputTokens;
                    var estimatedCost = (totalInputTokens / 1_000_000m * _inputCostPerMillionTokens) + (totalOutputTokens / 1_000_000m * _outputCostPerMillionTokens);

                    return new HrAssistantResult {
                        Answer = answer,
                        AssistantMessageId = assistantMessage.ChatMessageId,
                        Citations = BuildUsedCitations(answer, policySources),
                        ToolsUsed = toolsUsed,
                        InputTokens = totalInputTokens,
                        OutputTokens = totalOutputTokens,
                        TotalTokens = totalTokens,
                        EstimatedCost = estimatedCost
                    };
                }

                messages.Add(new AssistantChatMessage(completion));

                foreach (var toolCall in completion.ToolCalls) {
                    if (toolCall.FunctionName == "get_my_employee_profile") {
                        var profile = await _employeeProfileTool.GetMyEmployeeProfileAsync();
                        var result = profile is null ? JsonSerializer.Serialize(new { found = false }) : JsonSerializer.Serialize(new { found = true, profile.EmployeeNumber, profile.Department, profile.OfficeSchedule, profile.Location, profile.EmploymentStatus });

                        messages.Add(new ToolChatMessage(toolCall.Id, result));
                        toolsUsed.Add(toolCall.FunctionName);
                        continue;
                    }

                    if (toolCall.FunctionName == "search_hr_policy") {
                        policySearchCount++;

                        if (policySearchCount > 2) {
                            messages.Add(new ToolChatMessage(toolCall.Id, JsonSerializer.Serialize(new { found = false, mmessage = "The policy search limit has been reached. Do not offer another search. Answer using the authorized information already available. If that information does not answer the question, state that the requested information was not found in the HR information available to this user. Do not offer capabilities that are not provided by the available tools." })));
                            continue;
                        }

                        using var arguments = JsonDocument.Parse(toolCall.FunctionArguments);
                        var query = arguments.RootElement.GetProperty("query").GetString();

                        if (string.IsNullOrWhiteSpace(query))
                            throw new InvalidOperationException("search_hr_policy requires a query.");

                        var sources = await _hrPolicySearchTool.SearchAsync(query);
                        foreach (var source in sources) {
                            if (!policySources.Any(x => x.DocumentChunkId == source.DocumentChunkId))
                                policySources.Add(source);
                        }

                        var result = JsonSerializer.Serialize(sources);
                        messages.Add(new ToolChatMessage(toolCall.Id, result));
                        toolsUsed.Add(toolCall.FunctionName);
                        continue;
                    }

                    throw new InvalidOperationException($"Unsupported tool '{toolCall.FunctionName}'.");
                }
            }

            throw new InvalidOperationException("The HR assistant exceeded the maximum number of tool-calling rounds.");
        }

        private static List<RagCitation> BuildCitations(IReadOnlyList<HrPolicySearchResult> sources) {
            return sources.Select((source, index) => new RagCitation {
                SourceNumber = index + 1,
                DocumentId = source.DocumentId,
                DocumentChunkId = source.DocumentChunkId,
                FileName = source.FileName,
                Title = source.Title,
                Version = source.Version,
                PageNumber = source.PageNumber,
                SectionTitle = source.SectionTitle
            }).ToList();
        }

        private static List<RagCitation> BuildUsedCitations(string answer, IReadOnlyList<HrPolicySearchResult> sources) {
            return sources.Where(source => answer.Contains($"[Source {source.SourceNumber}]", StringComparison.OrdinalIgnoreCase)).Select(source => new RagCitation {
                SourceNumber = source.SourceNumber,
                DocumentId = source.DocumentId,
                DocumentChunkId = source.DocumentChunkId,
                FileName = source.FileName,
                Title = source.Title,
                Version = source.Version,
                PageNumber = source.PageNumber,
                SectionTitle = source.SectionTitle
            }).ToList();
        }
    }
}
