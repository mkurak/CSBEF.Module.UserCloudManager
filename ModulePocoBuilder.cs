using CSBEF.Core.Interfaces;
using CSBEF.Module.UserCloudManager.Models.Poco;
using Microsoft.EntityFrameworkCore;
using System;

namespace CSBEF.Module.UserCloudManager
{
    public class ModulePocoBuilder : ICustomModelBuilder
    {
        public void Build(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<FmDirectory>(entity =>
            {
                entity.ToTable("UserCloudManager_FmDirectory");

                entity.HasIndex(e => new { e.UserId, e.ParentDirectoryId, e.Title })
                    .HasName("IX_UserCloudManager_FmDirectory")
                    .IsUnique();

                entity.Property(e => e.AddingDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.FsKey).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Path)
                    .IsRequired()
                    .HasMaxLength(4000);

                entity.Property(e => e.Status).HasDefaultValueSql("((1))");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.UpdatingDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<FmFile>(entity =>
            {
                entity.ToTable("UserCloudManager_FmFile");

                entity.HasIndex(e => new { e.UserId, e.DirectoryId, e.Title })
                    .HasName("IX_UserCloudManager_FmFile")
                    .IsUnique();

                entity.Property(e => e.AddingDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.FsKey).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Path)
                    .IsRequired()
                    .HasMaxLength(4000);

                entity.Property(e => e.Status).HasDefaultValueSql("((1))");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.UpdatingDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<FmShare>(entity =>
            {
                entity.ToTable("UserCloudManager_FmShare");

                entity.Property(e => e.AddingDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Status).HasDefaultValueSql("((1))");

                entity.Property(e => e.UpdatingDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
            });
        }
    }
}
