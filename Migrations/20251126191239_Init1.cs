using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Support_Bot.Migrations
{

    public partial class Init1 : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    time_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    time_closed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ticket_content = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                    ticket_deleted_messages = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                    ticket_edited_messages = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                    ticket_creator_id = table.Column<string>(type: "text", nullable: false),
                    ticket_type = table.Column<string>(type: "text", nullable: false),
                    ticket_id = table.Column<string>(type: "text", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketLogs", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketLogs");
        }
    }
}
