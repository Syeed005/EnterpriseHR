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
        public DbSet<ChatSession> ChatSessions => Set<ChatSession>();

        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
        public DbSet<AnswerFeedback> AnswerFeedback { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<EmployeeProfile> EmployeeProfiles { get; set; }

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
                entity.Property(x => x.AccessLevel).HasMaxLength(50).IsRequired();
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
            
            modelBuilder.Entity<ChatSession>(entity =>
            {
                entity.HasKey(x => x.ChatSessionId);
                entity.HasOne(x => x.ApplicationUser)
                      .WithMany(x => x.ChatSessions)
                      .HasForeignKey(x => x.ApplicationUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(x => x.ChatMessageId);

                entity.Property(x => x.Role)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Content)
                    .IsRequired();

                entity.HasOne(x => x.ChatSession)
                    .WithMany(x => x.Messages)
                    .HasForeignKey(x => x.ChatSessionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasKey(x => x.ApplicationUserId);

                entity.Property(x => x.ExternalUserId)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Email)
                    .HasMaxLength(320)
                    .IsRequired();

                entity.Property(x => x.DisplayName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Role)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(x => x.ExternalUserId)
                    .IsUnique();
            });

            modelBuilder.Entity<AnswerFeedback>(entity =>
            {
                entity.HasKey(x => x.AnswerFeedbackId);

                entity.Property(x => x.Comment)
                    .HasMaxLength(1000);

                entity.HasOne(x => x.ChatMessage)
                    .WithMany(x => x.Feedback)
                    .HasForeignKey(x => x.ChatMessageId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ApplicationUser)
                    .WithMany(x => x.Feedback)
                    .HasForeignKey(x => x.ApplicationUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.ChatMessageId, x.ApplicationUserId })
                    .IsUnique();
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(x => x.AuditLogId);

                entity.Property(x => x.Action)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.ResourceType)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.ResourceId)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Details)
                    .HasMaxLength(2000);

                entity.HasOne(x => x.ApplicationUser)
                    .WithMany()
                    .HasForeignKey(x => x.ApplicationUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.CreatedAtUtc);
                entity.HasIndex(x => new { x.ResourceType, x.ResourceId });
            });

            modelBuilder.Entity<EmployeeProfile>(entity =>
            {
                entity.HasKey(x => x.EmployeeProfileId);

                entity.Property(x => x.EmployeeNumber)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Department)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.OfficeSchedule)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Location)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.EmploymentStatus)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(x => x.EmployeeNumber)
                    .IsUnique();

                entity.HasIndex(x => x.ApplicationUserId)
                    .IsUnique();

                entity.HasOne(x => x.ApplicationUser)
                    .WithOne()
                    .HasForeignKey<EmployeeProfile>(x => x.ApplicationUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
