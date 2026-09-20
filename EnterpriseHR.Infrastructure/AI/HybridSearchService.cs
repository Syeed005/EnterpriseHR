using EnterpriseHR.Core.AI;
using EnterpriseHR.Core.Observability;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.AI {
    public class HybridSearchService : IHybridSearchService {
        private readonly ISemanticSearchService _semanticSearchService;
        private readonly IFullTextSearchService _fullTextSearchService;

        public HybridSearchService(ISemanticSearchService semanticSearchService, IFullTextSearchService fullTextSearchService) {
            _semanticSearchService = semanticSearchService;
            _fullTextSearchService = fullTextSearchService;
        }

        public async Task<IReadOnlyList<RetrievalResult>> SearchAsync(string query, int topK = 3) {
            const int candidateK = 10;
            const int rrfK = 60;

            IReadOnlyList<SemanticSearchResult> semanticResults;
            using (var activity = EnterpriseHrTelemetry.ActivitySource.StartActivity("retrieval.semantic")) {
                semanticResults = await _semanticSearchService.SearchAsync(query, candidateK);

                activity?.SetTag("semantic.candidate_count", semanticResults.Count);
            }

            IReadOnlyList<FullTextSearchResult> fullTextResults;
            using (var activity = EnterpriseHrTelemetry.ActivitySource.StartActivity("retrieval.fulltext")) {
                fullTextResults = await _fullTextSearchService.SearchAsync(query, candidateK);

                activity?.SetTag("fulltext.candidate_count", fullTextResults.Count);
            }


            var results = new Dictionary<int, RetrievalResult>();

            using (var activity = EnterpriseHrTelemetry.ActivitySource.StartActivity("retrieval.rrf")) {
                for (var i = 0; i < semanticResults.Count; i++) {
                    var item = semanticResults[i];
                    var rank = i + 1;

                    results[item.DocumentChunkId] = new RetrievalResult {
                        DocumentChunkId = item.DocumentChunkId,
                        DocumentId = item.DocumentId,
                        PageNumber = item.PageNumber,
                        ChunkIndex = item.ChunkIndex,
                        SectionTitle = item.SectionTitle,
                        Content = item.Content,
                        SemanticRank = rank,
                        SemanticScore = item.RetrievalScore,
                        RrfScore = 1.0 / (rrfK + rank),
                        FileName = item.FileName,
                        Title = item.Title,
                        Version = item.Version
                    };
                }

                for (var i = 0; i < fullTextResults.Count; i++) {
                    var item = fullTextResults[i];
                    var rank = i + 1;

                    if (results.TryGetValue(item.DocumentChunkId, out var existing)) {
                        existing.FullTextRank = rank;
                        existing.FullTextScore = item.FullTextRank;
                        existing.RrfScore += 1.0 / (rrfK + rank);
                    } else {
                        results[item.DocumentChunkId] = new RetrievalResult {
                            DocumentChunkId = item.DocumentChunkId,
                            DocumentId = item.DocumentId,
                            PageNumber = item.PageNumber,
                            ChunkIndex = item.ChunkIndex,
                            SectionTitle = item.SectionTitle,
                            Content = item.Content,
                            FullTextRank = rank,
                            FullTextScore = item.FullTextRank,
                            RrfScore = 1.0 / (rrfK + rank),
                            FileName = item.FileName,
                            Title = item.Title,
                            Version = item.Version
                        };
                    }
                }
                activity?.SetTag("rrf.result_count", results.Count);
            }

                

            return results.Values
                .OrderByDescending(x => x.RrfScore)
                .ThenBy(x => x.SemanticRank ?? int.MaxValue)
                .Take(topK)
                .ToList();
        }
    }
}
