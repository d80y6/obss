using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.Audit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_alert_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    alert_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    threshold = table.Column<int>(type: "integer", nullable: false),
                    window_minutes = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_alert_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_alerts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    alert_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    entity_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    detected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_acknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    acknowledged_by_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    acknowledged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_alerts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    changes = table.Column<string>(type: "jsonb", nullable: true),
                    performed_by_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    performed_by_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    performed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_sensitive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_entries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    retention_days = table.Column<int>(type: "integer", nullable: false),
                    alert_on_failure = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_policies", x => x.id);
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

            migrationBuilder.CreateIndex(
                name: "ix_audit_alert_rules_alert_type",
                table: "audit_alert_rules",
                column: "alert_type");

            migrationBuilder.CreateIndex(
                name: "ix_audit_alert_rules_is_active",
                table: "audit_alert_rules",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_audit_alert_rules_tenant_id",
                table: "audit_alert_rules",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_alerts_alert_type",
                table: "audit_alerts",
                column: "alert_type");

            migrationBuilder.CreateIndex(
                name: "ix_audit_alerts_detected_at",
                table: "audit_alerts",
                column: "detected_at");

            migrationBuilder.CreateIndex(
                name: "ix_audit_alerts_is_acknowledged",
                table: "audit_alerts",
                column: "is_acknowledged");

            migrationBuilder.CreateIndex(
                name: "ix_audit_alerts_severity",
                table: "audit_alerts",
                column: "severity");

            migrationBuilder.CreateIndex(
                name: "ix_audit_alerts_tenant_id",
                table: "audit_alerts",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_entries_action",
                table: "audit_entries",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "ix_audit_entries_entity_type",
                table: "audit_entries",
                column: "entity_type");

            migrationBuilder.CreateIndex(
                name: "ix_audit_entries_entity_type_entity_id",
                table: "audit_entries",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_entries_performed_at",
                table: "audit_entries",
                column: "performed_at");

            migrationBuilder.CreateIndex(
                name: "ix_audit_entries_performed_by_id",
                table: "audit_entries",
                column: "performed_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_entries_tenant_id",
                table: "audit_entries",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_policies_entity_type",
                table: "audit_policies",
                column: "entity_type");

            migrationBuilder.CreateIndex(
                name: "ix_audit_policies_tenant_id",
                table: "audit_policies",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_policies_tenant_id_entity_type",
                table: "audit_policies",
                columns: new[] { "tenant_id", "entity_type" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_created_at",
                table: "outbox_messages",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_at",
                table: "outbox_messages",
                column: "processed_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_alert_rules");

            migrationBuilder.DropTable(
                name: "audit_alerts");

            migrationBuilder.DropTable(
                name: "audit_entries");

            migrationBuilder.DropTable(
                name: "audit_policies");

            migrationBuilder.DropTable(
                name: "outbox_messages");
        }
    }
}
