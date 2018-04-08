﻿using System;
using Microsoft.EntityFrameworkCore;

namespace AutoTagger.Database.Mysql
{
    using Microsoft.Extensions.Configuration;

    public partial class InstataggerContext : DbContext
    {
        public virtual DbSet<Itags> Itags { get; set; }
        public virtual DbSet<Mtags> Mtags { get; set; }
        public virtual DbSet<Photos> Photos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySql("Server=78.46.178.185;User Id=InstaTagger;Password=ovI5Aq3J0xOjjwXn;Database=instatagger");

                //string userName = Configuration.GetSection("AppConfiguration")["UserName"];
                //services.Configure<AppConfiguration>(Configuration.GetSection("AppConfiguration"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Itags>(entity =>
            {
                entity.ToTable("itags");

                entity.HasIndex(e => e.PhotoId)
                    .HasName("photoId");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.PhotoId)
                    .HasColumnName("photoId")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value")
                    .HasMaxLength(30);

                entity.HasOne(d => d.Photo)
                    .WithMany(p => p.Itags)
                    .HasForeignKey(d => d.PhotoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("itags_ibfk_1");
            });

            modelBuilder.Entity<Mtags>(entity =>
            {
                entity.ToTable("mtags");

                entity.HasIndex(e => e.PhotoId)
                    .HasName("photoId");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.PhotoId)
                    .HasColumnName("photoId")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value")
                    .HasMaxLength(30);

                entity.HasOne(d => d.Photo)
                    .WithMany(p => p.Mtags)
                    .HasForeignKey(d => d.PhotoId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("mtags_ibfk_1");
            });

            modelBuilder.Entity<Photos>(entity =>
            {
                entity.ToTable("photos");

                entity.HasIndex(e => e.Id)
                    .HasName("id")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Comments)
                    .HasColumnName("comments")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Follower)
                    .HasColumnName("follower")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Img)
                    .IsRequired()
                    .HasColumnName("img")
                    .HasMaxLength(50);

                entity.Property(e => e.Likes)
                    .HasColumnName("likes")
                    .HasColumnType("int(11)");
            });
        }
    }
}