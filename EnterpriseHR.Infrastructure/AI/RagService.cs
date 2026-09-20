using EnterpriseHR.Core.AI;
using EnterpriseHR.Core.Observability;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.AI {
    public class RagService : IRagService {
        private readonly IHybridSearchService _searchService;
        private readonly IChatService _chatService;
        private readonly IContextExpansionService _contextExpansionService;
        private readonly IAiCostCalculator _costCalculator;

        public RagService(IHybridSearchService searchService, IChatService chatService, IContextExpansionService contextExpansionService, IAiCostCalculator costCalculator) {
            _searchService = searchService;
            _chatService = chatService;
            _contextExpansionService = contextExpansionService;
            _costCalculator = costCalculator;
        }

        public async Task<RagAnswer> AskAsync(string question, int topK = 3) {
            using var activity = EnterpriseHrTelemetry.ActivitySource.StartActivity("rag.ask");
            activity?.SetTag("rag.top_k", topK);

            IReadOnlyList<RetrievalResult> searchResults;
            using (var searchActivity = EnterpriseHrTelemetry.ActivitySource.StartActivity("retrieval.hybrid")) {
                searchActivity?.SetTag("retrieval.top_k", topK);

                searchResults = await _searchService.SearchAsync(question, topK);

                searchActivity?.SetTag("retrieval.result_count", searchResults.Count);
            }

            IReadOnlyList<RetrievalResult> sources;
            using (var expansionActivity = EnterpriseHrTelemetry.ActivitySource.StartActivity("context.expand")) {
                expansionActivity?.SetTag("context.input_count", searchResults.Count);

                sources = await _contextExpansionService.ExpandAsync(searchResults);

                expansionActivity?.SetTag("context.output_count", sources.Count);
            }

            var prompt = BuildPrompt(question, sources);
            AiCostResult cost;

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

            var citations = sources
                .Select((source, index) => new RagCitation {
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
                })
            .ToList();

            return new RagAnswer {
                Answer = generation.Content,
                Citations = citations
            };
        }

        private static string BuildPrompt(string question, IReadOnlyList<RetrievalResult> sources) {
            var context = new StringBuilder();

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
                   
                   Answer the employee's question using only the supplied HR document context.
                   
                   Rules:
                   - Do not use outside knowledge.
                   - Do not invent policy details.
                   - If the supplied context does not contain enough information, say that the available HR documents do not provide enough information.
                   - Be concise and clear.
                   - Cite the relevant source numbers in the answer using [Source 1], [Source 2], etc.
                   
                   Employee question:
                   {question}
                   
                   HR document context:
                   {context}
                   
                   Answer:
                   """;
        }
    }
}
