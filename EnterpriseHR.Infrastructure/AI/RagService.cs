using EnterpriseHR.Core.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.AI {
    public class RagService : IRagService {
        private readonly ISemanticSearchService _searchService;
        private readonly IChatService _chatService;
        private readonly IContextExpansionService _contextExpansionService;

        public RagService(ISemanticSearchService searchService, IChatService chatService, IContextExpansionService contextExpansionService) {
            _searchService = searchService;
            _chatService = chatService;
            _contextExpansionService = contextExpansionService;
        }

        public async Task<RagAnswer> AskAsync(string question, int topK = 3) {
            var searchResults = await _searchService.SearchAsync(question, topK);
            var sources = await _contextExpansionService.ExpandAsync(searchResults);

            var prompt = BuildPrompt(question, sources);
            var answer = await _chatService.GenerateAnswerAsync(prompt);

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
                    RetrievalScore = source.RetrievalScore,
                    IsExpandedContext = source.IsExpandedContext
            })
            .ToList();

            return new RagAnswer {
                Answer = answer,
                Citations = citations
            };
        }

        private static string BuildPrompt(string question, IReadOnlyList<SemanticSearchResult> sources) {
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
