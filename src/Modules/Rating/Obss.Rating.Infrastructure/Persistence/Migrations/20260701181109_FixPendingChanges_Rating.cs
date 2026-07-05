using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.Rating.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingChanges_Rating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    handler_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    event_type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    event_data = table.Column<string>(type: "jsonb", nullable: false),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_messages", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_inbox_messages_event_id_handler_name",
                table: "inbox_messages",
                columns: new[] { "event_id", "handler_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inbox_messages_received_at",
                table: "inbox_messages",
                column: "received_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_messages");
        }
    }
}
