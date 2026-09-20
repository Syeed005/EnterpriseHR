using EnterpriseHR.Core.AI;
using EnterpriseHR.Core.Evaluation.Retrieval;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EnterpriseHR.Infrastructure.Evaluation {
    public class RetrievalEvaluationService : IRetrievalEvaluationService {
        private readonly IHybridSearchService _hybridSearchService;

        public RetrievalEvaluationService(IHybridSearchService hybridSearchService) {
            _hybridSearchService = hybridSearchService;
        }
        public async Task<RetrievalEvaluationSummary> RunAsync(int topK = 3) {
            var cases = GetEvaluationCases();
            var results = new List<RetrievalEvaluationResult>();

            foreach (var evaluationCase in cases) {
                var stopwatch = Stopwatch.StartNew();
                var searchResults = await _hybridSearchService.SearchAsync(evaluationCase.Question, topK);
                stopwatch.Stop();

                var retrievedChunkIds = searchResults
                    .Select(x => x.DocumentChunkId)
                    .ToList();

                var index = retrievedChunkIds.IndexOf(evaluationCase.ExpectedDocumentChunkId);

                results.Add(new RetrievalEvaluationResult {
                    Id = evaluationCase.Id,
                    Question = evaluationCase.Question,
                    ExpectedDocumentChunkId = evaluationCase.ExpectedDocumentChunkId,
                    ExpectedSectionTitle = evaluationCase.ExpectedSectionTitle,
                    FoundInTopK = index >= 0,
                    ActualRank = index >= 0 ? index + 1 : null,
                    RetrievedChunkIds = retrievedChunkIds,
                    LatencyMs = stopwatch.ElapsedMilliseconds
                });
            }

            var totalCases = results.Count;
            var passedCases = results.Count(r => r.FoundInTopK);
            var failedCases = totalCases - passedCases;
            var top1Hits = results.Count(r => r.ActualRank == 1);

            var reciprocalRankSum = results.Sum(r => r.ActualRank.HasValue ? 1.0 / r.ActualRank.Value : 0.0);

            var averageLatencyMs = results.Count > 0 ? results.Average(r => r.LatencyMs) : 0;
            var minLatencyMs = results.Count > 0 ? results.Min(r => r.LatencyMs) : 0;
            var maxLatencyMs = results.Count > 0 ? results.Max(r => r.LatencyMs) : 0;

            return new RetrievalEvaluationSummary {
                TopK = topK,
                TotalCases = totalCases,
                PassedCases = passedCases,
                FailedCases = failedCases,
                HitRateAtK = totalCases > 0 ? (double)passedCases / totalCases : 0,
                Top1HitRate = totalCases > 0 ? (double)top1Hits / totalCases : 0,
                MeanReciprocalRank = totalCases > 0 ? reciprocalRankSum / totalCases : 0,
                AverageLatencyMs = averageLatencyMs,
                MinLatencyMs = minLatencyMs,
                MaxLatencyMs = maxLatencyMs,
                Results = results
            };
        }

        private static IReadOnlyList<RetrievalEvaluationCase> GetEvaluationCases() {
            return
            [
                new()
            {
                Id = "EV001",
                Question = "Who is allowed to use the fitness centre?",
                ExpectedDocumentChunkId = 68,
                ExpectedSectionTitle = "01 Eligibility"
            },
            new()
            {
                Id = "EV002",
                Question = "How long can an employee use the fitness centre each day?",
                ExpectedDocumentChunkId = 69,
                ExpectedSectionTitle = "02 Operating Hours & Usage Duration"
            },
            new()
            {
                Id = "EV003",
                Question = "Do employees need to sign the register before using the fitness centre?",
                ExpectedDocumentChunkId = 71,
                ExpectedSectionTitle = "03 Registration"
            },
            new()
            {
                Id = "EV004",
                Question = "Can employees wear outdoor shoes inside the fitness centre?",
                ExpectedDocumentChunkId = 72,
                ExpectedSectionTitle = "04 Footwear"
            },
            new()
            {
                Id = "EV005",
                Question = "Where should employees keep their personal belongings?",
                ExpectedDocumentChunkId = 73,
                ExpectedSectionTitle = "06 Workout Attire"
            }
            ];
        }
    }
}
