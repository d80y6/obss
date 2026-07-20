using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.Reporting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dashboard_widgets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    widget_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    configuration = table.Column<string>(type: "jsonb", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    size = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data_source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    query = table.Column<string>(type: "text", nullable: false),
                    refresh_interval = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dashboard_widgets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    event_data = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "report_definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    report_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data_source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    query = table.Column<string>(type: "text", nullable: false),
                    output_format = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    schedule = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_report_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "report_executions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    report_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    file_size = table.Column<long>(type: "bigint", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    executed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_report_executions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scheduled_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    report_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cron_expression = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    recipients = table.Column<string[]>(type: "text[]", nullable: false),
                    last_run_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_run_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scheduled_reports", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_dashboard_widgets_tenant_id",
                table: "dashboard_widgets",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_dashboard_widgets_tenant_id_position",
                table: "dashboard_widgets",
                columns: new[] { "tenant_id", "position" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_created_at",
                table: "outbox_messages",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_at",
                table: "outbox_messages",
                column: "processed_at");

            migrationBuilder.CreateIndex(
                name: "ix_report_definitions_tenant_id",
                table: "report_definitions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_report_executions_report_definition_id",
                table: "report_executions",
                column: "report_definition_id");

            migrationBuilder.CreateIndex(
                name: "ix_scheduled_reports_next_run_at",
                table: "scheduled_reports",
                column: "next_run_at");

            migrationBuilder.CreateIndex(
                name: "ix_scheduled_reports_tenant_id",
                table: "scheduled_reports",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dashboard_widgets");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "report_definitions");

            migrationBuilder.DropTable(
                name: "report_executions");

            migrationBuilder.DropTable(
                name: "scheduled_reports");
        }
    }
}
