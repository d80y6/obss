using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.Workflow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "sla_definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    target_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    target_duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    escalation_duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    penalty_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    penalty_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sla_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workflow_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_instances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workflow_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    workflow_definition_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    trigger_entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    trigger_entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sla_deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sla_breached_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workflow_instances", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_metrics",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    metric_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    workflow_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    time_bucket = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workflow_metrics", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_slas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    workflow_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    warning_threshold_percent = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    escalation_user_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    escalation_group = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workflow_slas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_steps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workflow_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    step_number = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    step_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    handler_type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    configuration = table.Column<string>(type: "jsonb", nullable: true),
                    timeout = table.Column<int>(type: "integer", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    retry_delay_seconds = table.Column<int>(type: "integer", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workflow_steps", x => x.id);
                    table.ForeignKey(
                        name: "fk_workflow_steps_workflow_definitions_workflow_definition_id",
                        column: x => x.workflow_definition_id,
                        principalTable: "workflow_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_task_instances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workflow_instance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    workflow_step_id = table.Column<Guid>(type: "uuid", nullable: false),
                    step_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    assigned_to = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    result = table.Column<string>(type: "jsonb", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workflow_task_instances", x => x.id);
                    table.ForeignKey(
                        name: "fk_workflow_task_instances_workflow_instances_workflow_instanc",
                        column: x => x.workflow_instance_id,
                        principalTable: "workflow_instances",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_created_at",
                table: "outbox_messages",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_at",
                table: "outbox_messages",
                column: "processed_at");

            migrationBuilder.CreateIndex(
                name: "ix_sla_definitions_tenant_name",
                table: "sla_definitions",
                columns: new[] { "tenant_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workflow_definitions_tenant_name_version",
                table: "workflow_definitions",
                columns: new[] { "tenant_id", "name", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workflow_instances_status",
                table: "workflow_instances",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_workflow_instances_trigger_entity",
                table: "workflow_instances",
                columns: new[] { "trigger_entity_type", "trigger_entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_workflow_metrics_definition_type_bucket",
                table: "workflow_metrics",
                columns: new[] { "workflow_definition_id", "metric_type", "time_bucket" });

            migrationBuilder.CreateIndex(
                name: "ix_workflow_slas_tenant_name",
                table: "workflow_slas",
                columns: new[] { "tenant_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workflow_slas_workflow_definition_id",
                table: "workflow_slas",
                column: "workflow_definition_id");

            migrationBuilder.CreateIndex(
                name: "ix_workflow_steps_definition_step_number",
                table: "workflow_steps",
                columns: new[] { "workflow_definition_id", "step_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workflow_task_instances_instance_step",
                table: "workflow_task_instances",
                columns: new[] { "workflow_instance_id", "workflow_step_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workflow_task_instances_status",
                table: "workflow_task_instances",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "sla_definitions");

            migrationBuilder.DropTable(
                name: "workflow_metrics");

            migrationBuilder.DropTable(
                name: "workflow_slas");

            migrationBuilder.DropTable(
                name: "workflow_steps");

            migrationBuilder.DropTable(
                name: "workflow_task_instances");

            migrationBuilder.DropTable(
                name: "workflow_definitions");

            migrationBuilder.DropTable(
                name: "workflow_instances");
        }
    }
}
