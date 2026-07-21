using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.Rating.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_dead_lettered",
                table: "outbox_messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "last_error",
                table: "outbox_messages",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "lock_expires_at",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "lock_id",
                table: "outbox_messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "next_attempt_at",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "retry_count",
                table: "outbox_messages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "cdr_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    vendor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    source_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    normalized_data = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    error_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cdr_records", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_pending",
                table: "outbox_messages",
                columns: new[] { "processed_at", "next_attempt_at", "is_dead_lettered" });

            migrationBuilder.CreateIndex(
                name: "ix_cdr_records_correlation_id",
                table: "cdr_records",
                column: "correlation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cdr_records_received_at",
                table: "cdr_records",
                column: "received_at");

            migrationBuilder.CreateIndex(
                name: "ix_cdr_records_status",
                table: "cdr_records",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_cdr_records_vendor",
                table: "cdr_records",
                column: "vendor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cdr_records");

            migrationBuilder.DropIndex(
                name: "ix_outbox_messages_pending",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "is_dead_lettered",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "last_error",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "lock_expires_at",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "lock_id",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "next_attempt_at",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "retry_count",
                table: "outbox_messages");
        }
    }
}
