using EnterpriseHR.Core.AI;
using EnterpriseHR.Core.Documents;
using EnterpriseHR.Core.Evaluation;
using EnterpriseHR.Core.Services;
using EnterpriseHR.Infrastructure.AI;
using EnterpriseHR.Infrastructure.Data;
using EnterpriseHR.Infrastructure.Documents;
using EnterpriseHR.Infrastructure.Evaluation;
using EnterpriseHR.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

public partial class Program {
    private static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddDbContext<EnterpriseHrDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("EnterpriseHR")));

        builder.Services.AddScoped<IDocumentExtractor, PdfDocumentExtractor>();
        builder.Services.AddScoped<IDocumentIngestionService, DocumentIngestionService>();
        builder.Services.AddScoped<IDocumentTextNormalizer, DocumentTextNormalizer>();
        builder.Services.AddScoped<IDocumentMetadataExtractor, DocumentMetadataExtractor>();
        builder.Services.AddScoped<ITextChunker, TextChunker>();
        builder.Services.AddScoped<IDocumentChunkingService, DocumentChunkingService>();


        builder.Services.AddSingleton<IEmbeddingService>(sp => {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var apiKey = configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("OpenAI API key is not configured.");

            return new OpenAiEmbeddingService(apiKey);
        });

        builder.Services.AddScoped<IDocumentEmbeddingService, DocumentEmbeddingService>();
        builder.Services.AddScoped<ISemanticSearchService, SemanticSearchService>();

        builder.Services.AddSingleton<IChatService>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var apiKey = configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("OpenAI API key is not configured.");

            return new OpenAiChatService(apiKey);
        });

        builder.Services.AddScoped<IRagService, RagService>();
        builder.Services.AddScoped<IContextExpansionService, ContextExpansionService>();

        builder.Services.AddScoped<IFullTextSearchService, FullTextSearchService>();
        builder.Services.AddScoped<IHybridSearchService, HybridSearchService>();

        builder.Services.AddScoped<IDocumentLifecycleService, DocumentLifecycleService>();
        builder.Services.AddScoped<IRetrievalEvaluationService, RetrievalEvaluationService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.MapGet("/db-test", async (EnterpriseHrDbContext db) => {
            var canConnect = await db.Database.CanConnectAsync();
            return Results.Ok(new { canConnect });
        });

        app.MapPost("/documents/ingest", async (string filePath, IDocumentIngestionService ingestionService) => {
            var documentId = await ingestionService.IngestAsync(filePath);
            return Results.Ok(new { documentId });
        });

        app.MapPost("/documents/{documentId:int}/chunks", async (int documentId, IDocumentChunkingService chunkingService) => {
            var chunkCount = await chunkingService.ChunkDocumentAsync(documentId);
            return Results.Ok(new { documentId, chunkCount });
        });

        app.MapPost("/documents/{documentId:int}/embeddings", async (int documentId, IDocumentEmbeddingService embeddingService) => {
            var count = await embeddingService.GenerateEmbeddingsAsync(documentId);

            return Results.Ok(new {
                documentId,
                embeddingsGenerated = count
            });
        });

        app.MapGet("/search/semantic", async (string query, int? topK, ISemanticSearchService searchService) =>
        {
            var results = await searchService.SearchAsync(query, topK ?? 3);

            return Results.Ok(results);
        });

        app.MapPost("/chat/ask", async (RagQuestionRequest request, IRagService ragService) =>
        {
            var result = await ragService.AskAsync(request.Question, request.TopK);
            return Results.Ok(result);
        });

        app.MapGet("/search/fulltext", async (string query, int? topK, IFullTextSearchService searchService) =>
        {
            var results = await searchService.SearchAsync(query, topK ?? 5);
            return Results.Ok(results);
        });

        app.MapGet("/search/hybrid", async (string query, int? topK, IHybridSearchService searchService) =>
        {
            var results = await searchService.SearchAsync(query, topK ?? 3);
            return Results.Ok(results);
        });

        app.MapPost("/documents/{documentId:int}/activate", async (int documentId, IDocumentLifecycleService lifecycleService) => {
            await lifecycleService.ActivateAsync(documentId);
            return Results.Ok();
        });

        app.MapPost("/evaluation/retrieval", async (int? topK, IRetrievalEvaluationService evaluationService) =>
        {
            var results = await evaluationService.RunAsync(topK ?? 3);
            return Results.Ok(results);
        });

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}