using EnterpriseHR.Core.AI;
using EnterpriseHR.Core.Entities;
using EnterpriseHR.Core.Observability;
using EnterpriseHR.Core.Services;
using EnterpriseHR.Core.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EnterpriseHR.Infrastructure.AI {
    public class RagService : IRagService {
        private readonly IHybridSearchService _searchService;
        private readonly IChatService _chatService;
        private readonly IContextExpansionService _contextExpansionService;
        private readonly IAiCostCalculator _costCalculator;
        private readonly IAiUsageService _usageService;
        private readonly IConversationService _conversationService;
        private readonly IQuestionContextualizer _questionContextualizer;
        private readonly IEmployeeProfileTool _employeeProfileTool;

        public RagService(IHybridSearchService searchService, IChatService chatService, IContextExpansionService contextExpansionService, IAiCostCalculator costCalculator, IAiUsageService usageService, IConversationService conversationService, IQuestionContextualizer questionContextualizer, IEmployeeProfileTool employeeProfileTool) {
            _searchService = searchService;
            _chatService = chatService;
            _contextExpansionService = contextExpansionService;
            _costCalculator = costCalculator;
            _usageService = usageService;
            _conversationService = conversationService;
            _questionContextualizer = questionContextualizer;
            _employeeProfileTool = employeeProfileTool;
        }

        public async Task<RagAnswer> AskAsync(Guid sessionId, string question, int topK = 3) {
            using var activity = EnterpriseHrTelemetry.ActivitySource.StartActivity("rag.ask");
            activity?.SetTag("rag.top_k", topK);

            // Validate session + store question
            var history = await _conversationService.GetRecentMessagesAsync(sessionId, 6);
            await _conversationService.AddMessageAsync(sessionId, "user", question);


            string retrievalQuery;
            using (var contextualizeActivity = EnterpriseHrTelemetry.ActivitySource.StartActivity("conversation.contextualize")) {
                contextualizeActivity?.SetTag("conversation.history_count", history.Count);
                retrievalQuery = await _questionContextualizer.ContextualizeAsync(question, history);
            }

            var employeeProfile = await _employeeProfileTool.GetMyEmployeeProfileAsync();

            // RAG pipeline
            //Hybrid retrieval
            IReadOnlyList<RetrievalResult> searchResults;
            using (var searchActivity = EnterpriseHrTelemetry.ActivitySource.StartActivity("retrieval.hybrid")) {
                searchActivity?.SetTag("retrieval.top_k", topK);

                searchResults = await _searchService.SearchAsync(retrievalQuery, topK);

                searchActivity?.SetTag("retrieval.result_count", searchResults.Count);
            }

            //context expansion
            IReadOnlyList<RetrievalResult> sources;
            using (var expansionActivity = EnterpriseHrTelemetry.ActivitySource.StartActivity("context.expand")) {
                expansionActivity?.SetTag("context.input_count", searchResults.Count);

                sources = await _contextExpansionService.ExpandAsync(searchResults);

                expansionActivity?.SetTag("context.output_count", sources.Count);
            }

            //BuildPrompt
            var prompt = BuildPrompt(question, history, sources, employeeProfile);
            AiCostResult cost;

            //LLM
            ChatGenerationResult generation;
            using (var llmActivity = EnterpriseHrTelemetry.ActivitySource.StartActivity("llm.generate")) {
                generation = await _chatService.GenerateAnswerAsync(prompt);
                cost = _costCalculator.Calculate(generation.Model, generation.InputTokens, generation.OutputTokens);

                llmActivity?.SetTag("gen_ai.provider", generation.Provider);
                llmActivity?.SetTag("gen_ai.model", generation.Model);
                llmActivity?.SetTag("gen_ai.input_tokens", generation.InputTokens);
                llmActivity?.SetTag("gen_ai.output_tokens", generation.OutputTokens);
                llmActivity?.SetTag("gen_ai.total_tokens", generation.TotalTokens);
                llmActivity?.SetTag("gen_ai.input_cost_usd", (double)cost.InputCostUsd);
                llmActivity?.SetTag("gen_ai.output_cost_usd", (double)cost.OutputCostUsd);
                llmActivity?.SetTag("gen_ai.total_cost_usd", (double)cost.TotalCostUsd);
            }

            //citations
            var citations = sources.Select((source, index) => new RagCitation {
                SourceNumber = index + 1,
                DocumentId = source.DocumentId,
                DocumentChunkId = source.DocumentChunkId,
                FileName = source.FileName,
                Title = source.Title,
                Version = source.Version,
                PageNumber = source.PageNumber,
                SectionTitle = source.SectionTitle,

                SemanticRank = source.SemanticRank,
                SemanticScore = source.SemanticScore,

                FullTextRank = source.FullTextRank,
                FullTextScore = source.FullTextScore,

                RrfScore = source.RrfScore,

                IsExpandedContext = source.IsExpandedContext
            }).ToList();

            var answer = generation.Content;
            var assistantMessage = await _conversationService.AddMessageAsync(sessionId, "assistant", answer);

            await _usageService.RecordAsync(new AiUsageRecord {
                CreatedAtUtc = DateTime.UtcNow,
                Provider = generation.Provider,
                Model = generation.Model,
                InputTokens = generation.InputTokens,
                OutputTokens = generation.OutputTokens,
                TotalTokens = generation.TotalTokens,
                InputCostUsd = cost.InputCostUsd,
                OutputCostUsd = cost.OutputCostUsd,
                TotalCostUsd = cost.TotalCostUsd,
                Operation = "rag.chat",
                TraceId = Activity.Current?.TraceId.ToString()
            });

            return new RagAnswer {
                Answer = answer,
                AssistantMessageId = assistantMessage.ChatMessageId,
                Citations = citations
            };
        }

        private static string BuildPrompt(string question, IReadOnlyList<ChatMessage> history, IReadOnlyList<RetrievalResult> sources, EmployeeProfileToolResult? employeeProfile) {
            var context = new StringBuilder();

            var historyText = history.Count == 0 ? "No previous conversation." : string.Join("\n", history.Select(x => $"{x.Role}: {x.Content}"));

            var employeeContext = employeeProfile is null ? "No employee profile is available for the authenticated user." : $"""
                Employee Number: {employeeProfile.EmployeeNumber}
                Department: {employeeProfile.Department}
                Office Schedule: {employeeProfile.OfficeSchedule}
                Location: {employeeProfile.Location}
                Employment Status: {employeeProfile.EmploymentStatus}
                """;


            for (var i = 0; i < sources.Count; i++) {
                var source = sources[i];

                context.AppendLine($"[Source {i + 1}]");
                context.AppendLine($"Title: {source.Title ?? source.FileName}");
                context.AppendLine($"Version: {source.Version ?? "Unknown"}");
                context.AppendLine($"Page: {source.PageNumber}");
                context.AppendLine($"Section: {source.SectionTitle ?? "Unknown"}");
                context.AppendLine("Content:");
                context.AppendLine(source.Content);
                context.AppendLine();
            }

            return $"""
                You are an internal HR assistant.

                Answer the employee's current question using the supplied authenticated employee data and authoritative HR document context.

                Rules:
                - AUTHENTICATED EMPLOYEE DATA contains trusted structured information about the current authenticated employee.
                - Use AUTHENTICATED EMPLOYEE DATA only for facts about the current employee.
                - Use AUTHORITATIVE HR DOCUMENT CONTEXT for HR policies, rules, schedules, eligibility, and procedures.
                - When answering a personalized policy question, combine the authenticated employee data with the applicable HR policy information.
                - Use the conversation history to understand the employee's current question and maintain conversational continuity.
                - Previous assistant messages are not authoritative HR evidence.
                - Do not repeat information from previous assistant messages as fact unless supported by the authenticated employee data or supplied HR     document    context.
                - If conversation history conflicts with the supplied authoritative information, follow the supplied authoritative information.
                - Do not use outside knowledge.
                - Do not invent employee information or policy details.
                - If the supplied information is insufficient, say that the available employee data and HR documents do not provide enough information.
                - Cite HR policy claims using [Source 1], [Source 2], etc.
                - Do not create document citations for authenticated employee data.
                - Be concise and clear.

                Conversation history:
                {historyText}

                AUTHENTICATED EMPLOYEE DATA:
                {employeeContext}

                Current employee question:
                {question}

                AUTHORITATIVE HR DOCUMENT CONTEXT:
                {context}

                Answer:
                """;
        }
    }
}
