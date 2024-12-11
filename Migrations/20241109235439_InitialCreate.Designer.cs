﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using pwmgr_backend.Data;

#nullable disable

namespace pwmgr_backend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20241109235439_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("pwmgr_backend.Models.PasswordEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("EncryptedMetadata")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("EncryptedPassword")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("EncryptedPasswordKey")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("HkdfSalt")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("PasswordEntries");
                });

            modelBuilder.Entity("pwmgr_backend.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Salt")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("pwmgr_backend.Models.PasswordEntry", b =>
                {
                    b.HasOne("pwmgr_backend.Models.User", "User")
                        .WithMany("PasswordEntries")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("pwmgr_backend.Models.User", b =>
                {
                    b.Navigation("PasswordEntries");
                });
#pragma warning restore 612, 618
        }
    }
}