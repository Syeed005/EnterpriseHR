using EnterpriseHR.Core.Documents;
using EnterpriseHR.Core.Services;
using EnterpriseHR.Infrastructure.Data;
using EnterpriseHR.Infrastructure.Documents;
using EnterpriseHR.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
