using EnterpriseHR.Core.AI;
using EnterpriseHR.Core.Evaluation.Rag;
using EnterpriseHR.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EnterpriseHR.Infrastructure.Evaluation.Rag {
    public class RagEvaluationService : IRagEvaluationService {
        private readonly IRagService _ragService;
        private readonly IConversationService _conversationService;
        public RagEvaluationService(IRagService ragService, IConversationService conversationService) {
            _ragService = ragService;
            _conversationService = conversationService;
        }

        public async Task<RagEvaluationSummary> RunAsync() {
            var cases = GetEvaluationCases();
            var results = new List<RagEvaluationResult>();

            foreach (var evaluationCase in cases) {
                var session = await _conversationService.CreateSessionAsync();
                var stopwatch = Stopwatch.StartNew();

                var ragResponse = await _ragService.AskAsync(session.ChatSessionId, evaluationCase.Question,3);

                stopwatch.Stop();

                var normalizedAnswer = NormalizeForComparison(ragResponse.Answer);
                var matchedFacts = evaluationCase.ExpectedAnswerFacts
                    .Where(fact => normalizedAnswer.Contains(
                        NormalizeForComparison(fact),
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var missingFacts = evaluationCase.ExpectedAnswerFacts
                    .Where(fact => !normalizedAnswer.Contains(
                        NormalizeForComparison(fact),
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var expectedCitationFound = ragResponse.Citations
                    .Any(c => c.DocumentChunkId == evaluationCase.ExpectedDocumentChunkId);

                results.Add(new RagEvaluationResult {
                    Id = evaluationCase.Id,
                    Question = evaluationCase.Question,
                    Answer = ragResponse.Answer,
                    ExpectedAnswerFacts = evaluationCase.ExpectedAnswerFacts,
                    MatchedFacts = matchedFacts,
                    MissingFacts = missingFacts,
                    AllExpectedFactsFound = missingFacts.Count == 0,
                    ExpectedDocumentChunkId = evaluationCase.ExpectedDocumentChunkId,
                    ExpectedCitationFound = expectedCitationFound,
                    LatencyMs = stopwatch.ElapsedMilliseconds
                });
            }

            var totalCases = results.Count;

            var passedCases = results.Count(r => r.AllExpectedFactsFound && r.ExpectedCitationFound);

            var failedCases = totalCases - passedCases;

            var answerFactSuccessCount = results.Count(r => r.AllExpectedFactsFound);
            var citationSuccessCount = results.Count(r => r.ExpectedCitationFound);

            return new RagEvaluationSummary {
                TotalCases = totalCases,
                PassedCases = passedCases,
                FailedCases = failedCases,

                AnswerFactSuccessRate = totalCases > 0 ? (double)answerFactSuccessCount / totalCases : 0,
                CitationSuccessRate = totalCases > 0 ? (double)citationSuccessCount / totalCases : 0,
                AverageLatencyMs = totalCases > 0 ? results.Average(r => r.LatencyMs) : 0,
                MinLatencyMs = totalCases > 0 ? results.Min(r => r.LatencyMs) : 0,
                MaxLatencyMs = totalCases > 0 ? results.Max(r => r.LatencyMs) : 0,

                Results = results
            };
        }

        private static IReadOnlyList<RagEvaluationCase> GetEvaluationCases() {
            return
            [
                new()
            {
                Id = "EV001",
                Question = "Who is allowed to use the fitness centre?",
                ExpectedDocumentChunkId = 68,
                ExpectedAnswerFacts =
                [
                    "BJIT employees",
                    "first-come, first-served"
                ]
            },

            new()
            {
                Id = "EV002",
                Question = "How long can an employee use the fitness centre each day?",
                ExpectedDocumentChunkId = 69,
                ExpectedAnswerFacts =
                [
                    "45 minutes"
                ]
            },

            new()
            {
                Id = "EV003",
                Question = "Do employees need to sign the register before using the fitness centre?",
                ExpectedDocumentChunkId = 71,
                ExpectedAnswerFacts =
                [
                    "sign",
                    "register"
                ]
            },

            new()
            {
                Id = "EV004",
                Question = "Can employees wear outdoor shoes inside the fitness centre?",
                ExpectedDocumentChunkId = 72,
                ExpectedAnswerFacts =
                [
                    "outdoor shoes",
                    "indoor gym shoes"
                ]
            },

            new()
            {
                Id = "EV005",
                Question = "Where should employees keep their personal belongings?",
                ExpectedDocumentChunkId = 73,
                ExpectedAnswerFacts =
                [
                    "designated storage area"
                ]
            }
            ];
        }

        private static string NormalizeForComparison(string text) {
            return text
                .Replace('-', '-')
                .Replace('–', '-')
                .Replace('—', '-')
                .Replace('\u00A0', ' ')
                .Trim();
        }
    }
}
