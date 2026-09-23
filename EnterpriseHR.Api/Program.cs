using EnterpriseHR.Api.ExceptionHandling;
using EnterpriseHR.Api.Security;
using EnterpriseHR.Api.Validation;
using EnterpriseHR.Core.AI;
using EnterpriseHR.Core.Documents;
using EnterpriseHR.Core.Evaluation.AgentEvaluation;
using EnterpriseHR.Core.Evaluation.Conversation;
using EnterpriseHR.Core.Evaluation.Rag;
using EnterpriseHR.Core.Evaluation.Retrieval;
using EnterpriseHR.Core.Feedback;
using EnterpriseHR.Core.Models;
using EnterpriseHR.Core.Observability;
using EnterpriseHR.Core.Requests;
using EnterpriseHR.Core.Security;
using EnterpriseHR.Core.Services;
using EnterpriseHR.Core.Tools;
using EnterpriseHR.Infrastructure.AI;
using EnterpriseHR.Infrastructure.Data;
using EnterpriseHR.Infrastructure.Documents;
using EnterpriseHR.Infrastructure.Evaluation.AgentEvaluation;
using EnterpriseHR.Infrastructure.Evaluation.Conversation;
using EnterpriseHR.Infrastructure.Evaluation.Rag;
using EnterpriseHR.Infrastructure.Evaluation.Retrieval;
using EnterpriseHR.Infrastructure.Security;
using EnterpriseHR.Infrastructure.Services;
using EnterpriseHR.Infrastructure.Tools;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;

public partial class Program {
    private static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);
        
        if (!builder.Environment.IsDevelopment())
            throw new InvalidOperationException("Production authentication is not configured. Configure the approved production identity provider before running outside Development.");


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
        builder.Services.AddScoped<IRagEvaluationService, RagEvaluationService>();
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("EnterpriseHR.Api"))
            .WithTracing(tracing => {
                tracing
                    .AddSource(EnterpriseHrTelemetry.ActivitySourceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter();
            });
        builder.Services.AddSingleton<IAiCostCalculator, AiCostCalculator>();
        builder.Services.AddScoped<IAiUsageService, AiUsageService>();
        builder.Services.AddScoped<IConversationService, ConversationService>();

        builder.Services.AddSingleton<IQuestionContextualizer>(sp => {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var apiKey = configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("OpenAI API key is not configured.");

            return new OpenAiQuestionContextualizer(apiKey);
        });

        builder.Services.AddScoped<IConversationEvaluationService, ConversationEvaluationService>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();

        if (builder.Environment.IsDevelopment()) {
            builder.Services.AddAuthentication(DevelopmentAuthHandler.SchemeName)
                            .AddScheme<AuthenticationSchemeOptions, DevelopmentAuthHandler>(DevelopmentAuthHandler.SchemeName, options => { });
        }

            

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.EmployeeAccess, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new ApplicationRoleRequirement(
                    ApplicationRoles.Employee,
                    ApplicationRoles.HR,
                    ApplicationRoles.Admin));
            });

            options.AddPolicy(AuthorizationPolicies.HRAccess, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new ApplicationRoleRequirement(
                    ApplicationRoles.HR,
                    ApplicationRoles.Admin));
            });

            options.AddPolicy(AuthorizationPolicies.AdminAccess, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new ApplicationRoleRequirement(
                    ApplicationRoles.Admin));
            });
        });

        builder.Services.AddScoped<IAuthorizationHandler, ApplicationRoleAuthorizationHandler>();
        builder.Services.AddScoped<IDocumentAccessService, DocumentAccessService>();

        builder.Services.AddScoped<IFeedbackService, FeedbackService>();
        builder.Services.AddScoped<IAuditService, AuditService>();

        builder.Services.AddScoped<IEmployeeProfileService, EmployeeProfileService>();
        builder.Services.AddScoped<IEmployeeProfileTool, EmployeeProfileTool>();

        builder.Services.AddScoped<IToolCallingService>(sp => new OpenAiToolCallingService(builder.Configuration["OpenAI:ApiKey"]!, sp.GetRequiredService<IEmployeeProfileTool>()));

        builder.Services.AddScoped<IHrPolicySearchTool, HrPolicySearchTool>();

        builder.Services.AddScoped<IHrAssistantService>(sp => new HrAssistantService(builder.Configuration["OpenAI:ApiKey"]!, sp.GetRequiredService<IEmployeeProfileTool>(), sp.GetRequiredService<IHrPolicySearchTool>(), sp.GetRequiredService<IConversationService>()));

        builder.Services.AddScoped<IAgentEvaluationService, AgentEvaluationService>();

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

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
        }).RequireAuthorization(AuthorizationPolicies.HRAccess);

        app.MapPost("/documents/{documentId:int}/chunks", async (int documentId, IDocumentChunkingService chunkingService) => {
            var chunkCount = await chunkingService.ChunkDocumentAsync(documentId);
            return Results.Ok(new { documentId, chunkCount });
        }).RequireAuthorization(AuthorizationPolicies.HRAccess);

        app.MapPost("/documents/{documentId:int}/embeddings", async (int documentId, IDocumentEmbeddingService embeddingService) => {
            var count = await embeddingService.GenerateEmbeddingsAsync(documentId);

            return Results.Ok(new {
                documentId,
                embeddingsGenerated = count
            });
        }).RequireAuthorization(AuthorizationPolicies.HRAccess);

        app.MapPost("/documents/{documentId:int}/activate", async (int documentId, IDocumentLifecycleService lifecycleService) => {
            await lifecycleService.ActivateAsync(documentId);
            return Results.Ok();
        }).RequireAuthorization(AuthorizationPolicies.HRAccess);

        app.MapGet("/search/semantic", async (string query, int? topK, ISemanticSearchService searchService) =>
        {
            var results = await searchService.SearchAsync(query, topK ?? 3);

            return Results.Ok(results);
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

        

        app.MapPost("/evaluation/retrieval", async (int? topK, IRetrievalEvaluationService evaluationService) =>
        {
            var summary = await evaluationService.RunAsync(topK ?? 3);
            return Results.Ok(summary);
        });

        app.MapPost("/evaluation/rag", async (IRagEvaluationService evaluationService) =>
        {
            var summary = await evaluationService.RunAsync();
            return Results.Ok(summary);
        });

        app.MapPost("/evaluation/conversation", async (IConversationEvaluationService evaluationService) => {
            var result = await evaluationService.RunAsync();
            return Results.Ok(result);
        });

        if (app.Environment.IsDevelopment()) {
            app.MapPost("/evaluation/agent", async (Guid sessionId, IAgentEvaluationService evaluationService, CancellationToken cancellationToken) => {
                var results = await evaluationService.RunAsync(sessionId, cancellationToken);
                return Results.Ok(results);
            }).RequireAuthorization(AuthorizationPolicies.EmployeeAccess);
        }

        app.MapGet("/usage/summary", async (IAiUsageService usageService) =>
        {
            var summary = await usageService.GetSummaryAsync();
            return Results.Ok(summary);
        });

        app.MapGet("/usage/summary/range", async (DateTime? fromUtc, DateTime? toUtc, IAiUsageService usageService) =>
        {
            var summary = await usageService.GetSummaryAsync(fromUtc, toUtc);
            return Results.Ok(summary);
        });

        app.MapPost("/chat/sessions", async (IConversationService conversationService) =>
        {
            var session = await conversationService.CreateSessionAsync();

            return Results.Ok(new {
                session.ChatSessionId,
                session.CreatedAtUtc
            });
        }).RequireAuthorization(AuthorizationPolicies.EmployeeAccess);

        app.MapPost("/chat/ask", async (AskHrAssistantRequest request, IHrAssistantService hrAssistantService, CancellationToken cancellationToken) => {
            RequestValidator.Validate(request);
            return Results.Ok(await hrAssistantService.AskAsync(request.SessionId, request.Question));
        }).RequireAuthorization(AuthorizationPolicies.EmployeeAccess);

        app.MapPost("/chat/sessions/{sessionId:guid}/messages", async (Guid sessionId, string role, string content, IConversationService conversationService) =>
        {
            var message = await conversationService.AddMessageAsync(sessionId, role, content);

            return Results.Ok(new {
                messageId = message.ChatMessageId,
                sessionId = message.ChatSessionId,
                role = message.Role,
                content = message.Content,
                createdAtUtc = message.CreatedAtUtc
            });
        }).RequireAuthorization(AuthorizationPolicies.EmployeeAccess);

        app.MapGet("/chat/sessions/{sessionId:guid}/messages", async (Guid sessionId, int? count, IConversationService conversationService) =>
        {
            var messages = await conversationService.GetRecentMessagesAsync(sessionId, count ?? 6);

            return Results.Ok(messages.Select(message => new
            {
                messageId = message.ChatMessageId,
                sessionId = message.ChatSessionId,
                role = message.Role,
                content = message.Content,
                createdAtUtc = message.CreatedAtUtc
            }));
        }).RequireAuthorization(AuthorizationPolicies.EmployeeAccess);

        

        app.MapGet("/auth/me", async (ICurrentUserService currentUserService, IApplicationUserService applicationUserService) =>
        {
            var user = await applicationUserService.GetOrCreateCurrentUserAsync();

            return Results.Ok(new {
                currentUserService.IsAuthenticated,
                currentUserService.ExternalUserId,
                currentUserService.Email,
                currentUserService.DisplayName,
                user.ApplicationUserId,
                user.Role
            });
        }).RequireAuthorization();

        app.MapGet("/auth/hr-test", () =>
        {
            return Results.Ok(new { message = "HR authorization passed." });
        }).RequireAuthorization(AuthorizationPolicies.HRAccess);

        app.MapPost("/feedback", async (SubmitFeedbackRequest request, IFeedbackService feedbackService) =>
        {
            var feedback = await feedbackService.SubmitAsync(request);

            return Results.Ok(new {
                feedback.AnswerFeedbackId,
                feedback.ChatMessageId,
                feedback.IsHelpful,
                feedback.Comment,
                feedback.CreatedAtUtc
            });
        }).RequireAuthorization(AuthorizationPolicies.EmployeeAccess);

        app.MapGet("/employees/me/profile", async (IEmployeeProfileService employeeProfileService) =>
        {
            var profile = await employeeProfileService.GetMyProfileAsync();

            if (profile is null)
                return Results.NotFound();

            return Results.Ok(new {
                profile.EmployeeNumber,
                profile.Department,
                profile.OfficeSchedule,
                profile.Location,
                profile.EmploymentStatus
            });
        }).RequireAuthorization(AuthorizationPolicies.EmployeeAccess);

        //app.MapPost("/tools/ask", async (string question, IToolCallingService toolCallingService) => 
        //    Results.Ok(await toolCallingService.AskAsync(question))).RequireAuthorization(AuthorizationPolicies.EmployeeAccess);

        //app.MapPost("/agent/ask", async (Guid sessionId, string question, IHrAssistantService hrAssistantService) => Results.Ok(await hrAssistantService.AskAsync(sessionId, question))).RequireAuthorization(AuthorizationPolicies.EmployeeAccess);
        app.UseExceptionHandler();

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();

        app.Run();
    }
}