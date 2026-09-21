using EnterpriseHR.Core.AI;
using EnterpriseHR.Core.Evaluation.Conversation;
using EnterpriseHR.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EnterpriseHR.Infrastructure.Evaluation.Conversation {
    public class ConversationEvaluationService : IConversationEvaluationService {
        private readonly IRagService _ragService;
        private readonly IConversationService _conversationService;

        public ConversationEvaluationService(IRagService ragService, IConversationService conversationService) {
            _ragService = ragService;
            _conversationService = conversationService;
        }

        public async Task<ConversationEvaluationSummary> RunAsync() {
            var turns = GetEvaluationTurns();

            // One session for the entire conversation.
            var session = await _conversationService.CreateSessionAsync();

            var results = new List<ConversationEvaluationResult>();

            foreach (var turn in turns) {
                var stopwatch = Stopwatch.StartNew();

                var response = await _ragService.AskAsync(session.ChatSessionId, turn.Question, 3);

                stopwatch.Stop();

                var matchedFacts = turn.ExpectedAnswerFacts
                    .Where(fact => ContainsNormalized(response.Answer, fact))
                    .ToList();

                var missingFacts = turn.ExpectedAnswerFacts
                    .Where(fact => !ContainsNormalized(response.Answer, fact))
                    .ToList();

                var citationFound = response.Citations.Any(citation =>
                    citation.DocumentChunkId == turn.ExpectedDocumentChunkId);

                results.Add(new ConversationEvaluationResult {
                    Id = turn.Id,
                    Question = turn.Question,
                    Answer = response.Answer,
                    ExpectedAnswerFacts = turn.ExpectedAnswerFacts,
                    MatchedFacts = matchedFacts,
                    MissingFacts = missingFacts,
                    AllExpectedFactsFound = missingFacts.Count == 0,
                    ExpectedDocumentChunkId = turn.ExpectedDocumentChunkId,
                    ExpectedCitationFound = citationFound,
                    LatencyMs = stopwatch.ElapsedMilliseconds
                });
            }

            var passedTurns = results.Count(x =>
                x.AllExpectedFactsFound && x.ExpectedCitationFound);

            return new ConversationEvaluationSummary {
                TotalTurns = results.Count,
                PassedTurns = passedTurns,
                FailedTurns = results.Count - passedTurns,

                AnswerFactSuccessRate = results.Count == 0
                    ? 0
                    : (double)results.Count(x => x.AllExpectedFactsFound) / results.Count,

                CitationSuccessRate = results.Count == 0
                    ? 0
                    : (double)results.Count(x => x.ExpectedCitationFound) / results.Count,

                AverageLatencyMs = results.Count == 0
                    ? 0
                    : results.Average(x => x.LatencyMs),

                MinLatencyMs = results.Count == 0
                    ? 0
                    : results.Min(x => x.LatencyMs),

                MaxLatencyMs = results.Count == 0
                    ? 0
                    : results.Max(x => x.LatencyMs),

                Results = results
            };
        }

        private static List<ConversationEvaluationTurn> GetEvaluationTurns() {
            return
            [
                new()
            {
                Id = "CV001-T1",
                Question = "I'm on the 8:00 AM to 5:00 PM office schedule. When can I use the fitness centre?",
                ExpectedAnswerFacts = ["6:00 AM", "7:45 AM", "5:00 PM"],
                ExpectedDocumentChunkId = 69
            },
            new()
            {
                Id = "CV001-T2",
                Question = "What about Friday after work?",
                ExpectedAnswerFacts = ["5:30 PM", "7:45 PM"],
                ExpectedDocumentChunkId = 69
            },
            new()
            {
                Id = "CV001-T3",
                Question = "How long can I stay?",
                ExpectedAnswerFacts = ["45 minutes"],
                ExpectedDocumentChunkId = 69
            }
            ];
        }

        private static bool ContainsNormalized(string text, string expected) {
            return Normalize(text).Contains(Normalize(expected), StringComparison.OrdinalIgnoreCase);
        }

        private static string Normalize(string value) {
            return value
                .Replace('\u2011', '-')
                .Replace('\u2013', '-')
                .Replace('\u2014', '-')
                .Replace('\u00A0', ' ')
                .Trim();
        }
    }
}
