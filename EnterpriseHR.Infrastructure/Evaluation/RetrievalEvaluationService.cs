using EnterpriseHR.Core.AI;
using EnterpriseHR.Core.Evaluation;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.Evaluation {
    public class RetrievalEvaluationService : IRetrievalEvaluationService {
        private readonly IHybridSearchService _hybridSearchService;

        public RetrievalEvaluationService(IHybridSearchService hybridSearchService) {
            _hybridSearchService = hybridSearchService;
        }
        public async Task<IReadOnlyList<RetrievalEvaluationResult>> RunAsync(int topK = 3) {
            var cases = GetEvaluationCases();
            var results = new List<RetrievalEvaluationResult>();

            foreach (var evaluationCase in cases) {
                var searchResults = await _hybridSearchService.SearchAsync(evaluationCase.Question, topK);

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
                    RetrievedChunkIds = retrievedChunkIds
                });
            }

            return results;
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
