
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Support_Bot.DB_METHODS.ContextAndHelpers;

#nullable disable

namespace Support_Bot.Migrations
{
    [DbContext(typeof(ContextForDB))]
    [Migration("20251126191239_Init1")]
    partial class Init1
    {

        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Support_Bot.DB_METHODS.Entitys.TicketLogs", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("isactive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.Property<string>("ticket_content")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("");

                    b.Property<string>("ticket_creator_id")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ticket_deleted_messages")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("");

                    b.Property<string>("ticket_edited_messages")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("");

                    b.Property<string>("ticket_id")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ticket_type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("time_closed")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("time_created")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("TicketLogs");
                });
#pragma warning restore 612, 618
        }
    }
}
