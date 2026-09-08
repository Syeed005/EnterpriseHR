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
            });

            modelBuilder.Entity<DocumentPage>(entity =>
            {
                entity.ToTable("DocumentPages");
                entity.HasKey(x => x.DocumentPageId);

                entity.Property(x => x.Content).IsRequired();

                entity.HasOne(x => x.Document)
                    .WithMany(x => x.Pages)
                    .HasForeignKey(x => x.DocumentId);
            });
        }
    }
}
