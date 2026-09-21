using EnterpriseHR.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseHR.Infrastructure.Data {
    public class EnterpriseHrDbContext : DbContext {
        public EnterpriseHrDbContext(DbContextOptions<EnterpriseHrDbContext> options) : base(options) {
        }

        public DbSet<Document> Documents => Set<Document>();
        public DbSet<DocumentPage> DocumentPages => Set<DocumentPage>();
        public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();
        public DbSet<AiUsageRecord> AiUsageRecords => Set<AiUsageRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Document>(entity =>
            {
                entity.ToTable("Documents");
                entity.HasKey(x => x.DocumentId);

                entity.Property(x => x.FileName).HasMaxLength(260).IsRequired();
                entity.Property(x => x.FilePath).HasMaxLength(1000).IsRequired();
                entity.Property(x => x.FileType).HasMaxLength(20).IsRequired();
                entity.Property(x => x.FileHash).HasMaxLength(64);
                entity.Property(x => x.Title).HasMaxLength(500);
                entity.Property(x => x.DocumentType).HasMaxLength(100);
                entity.Property(x => x.Version).HasMaxLength(50);
                entity.Property(x => x.IssuedBy).HasMaxLength(200);
                entity.Property(x => x.Audience).HasMaxLength(200);
                entity.Property(x => x.Status).HasMaxLength(50);
                entity.Property(x => x.PolicyKey).HasMaxLength(150);
            });

            modelBuilder.Entity<DocumentPage>(entity =>
            {
                entity.ToTable("DocumentPages");
                entity.HasKey(x => x.DocumentPageId);

                entity.Property(x => x.RawContent).IsRequired();
                entity.Property(x => x.NormalizedContent).IsRequired();

                entity.HasOne(x => x.Document)
                    .WithMany(x => x.Pages)
                    .HasForeignKey(x => x.DocumentId);
            });

            modelBuilder.Entity<DocumentChunk>(entity =>
            {
                entity.ToTable("DocumentChunks");
                entity.HasKey(x => x.DocumentChunkId);

                entity.Property(x => x.Content).IsRequired();
                entity.Property(x => x.SectionTitle).HasMaxLength(500);
                entity.HasIndex(x => new { x.DocumentPageId, x.ChunkIndex }).IsUnique();
                entity.Property(x => x.EmbeddingJson);

                entity.HasOne(x => x.DocumentPage)
                    .WithMany(x => x.Chunks)
                    .HasForeignKey(x => x.DocumentPageId);
            });

            modelBuilder.Entity<AiUsageRecord>(entity =>
            {
                entity.HasKey(x => x.AiUsageRecordId);

                entity.Property(x => x.Provider)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Model)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Operation)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.TraceId)
                    .HasMaxLength(100);

                entity.Property(x => x.InputCostUsd)
                    .HasPrecision(18, 10);

                entity.Property(x => x.OutputCostUsd)
                    .HasPrecision(18, 10);

                entity.Property(x => x.TotalCostUsd)
                    .HasPrecision(18, 10);
            });
        }
    }
}
