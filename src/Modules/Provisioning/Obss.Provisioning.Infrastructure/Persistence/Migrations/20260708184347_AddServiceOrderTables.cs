using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.Provisioning.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceOrderTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "service_specification_id",
                table: "provisioning_templates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_base_type",
                table: "provisioning_jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_schema_location",
                table: "provisioning_jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_type",
                table: "provisioning_jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "correlation_id",
                table: "provisioning_jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "provisioning_jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "href",
                table: "provisioning_jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "priority",
                table: "provisioning_jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "related_party_id",
                table: "provisioning_jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "related_party_name",
                table: "provisioning_jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "related_party_role",
                table: "provisioning_jobs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "service_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    requested_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    requested_completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status_change_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completion_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    href = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cancel_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cancel_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    cancel_completed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancel_state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "service_order_characteristic",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    service_order_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_order_characteristic", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_order_characteristic_service_orders_service_order_id",
                        column: x => x.service_order_id,
                        principalTable: "service_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    requested_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    requested_completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_order_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_order_items_service_orders_service_order_id",
                        column: x => x.service_order_id,
                        principalTable: "service_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_order_milestone",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    service_order_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_order_milestone", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_order_milestone_service_orders_service_order_id",
                        column: x => x.service_order_id,
                        principalTable: "service_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_order_note",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    author = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    service_order_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_order_note", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_order_note_service_orders_service_order_id",
                        column: x => x.service_order_id,
                        principalTable: "service_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_order_related_party",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    role = table.Column<string>(type: "text", nullable: true),
                    party_id = table.Column<string>(type: "text", nullable: true),
                    service_order_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_order_related_party", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_order_related_party_service_orders_service_order_id",
                        column: x => x.service_order_id,
                        principalTable: "service_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_service_order_characteristic_service_order_id",
                table: "service_order_characteristic",
                column: "service_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_order_items_service_order_id",
                table: "service_order_items",
                column: "service_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_order_milestone_service_order_id",
                table: "service_order_milestone",
                column: "service_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_order_note_service_order_id",
                table: "service_order_note",
                column: "service_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_order_related_party_service_order_id",
                table: "service_order_related_party",
                column: "service_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_orders_external_id",
                table: "service_orders",
                column: "external_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_orders_state",
                table: "service_orders",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "ix_service_orders_tenant_id",
                table: "service_orders",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "service_order_characteristic");

            migrationBuilder.DropTable(
                name: "service_order_items");

            migrationBuilder.DropTable(
                name: "service_order_milestone");

            migrationBuilder.DropTable(
                name: "service_order_note");

            migrationBuilder.DropTable(
                name: "service_order_related_party");

            migrationBuilder.DropTable(
                name: "service_orders");

            migrationBuilder.DropColumn(
                name: "service_specification_id",
                table: "provisioning_templates");

            migrationBuilder.DropColumn(
                name: "at_base_type",
                table: "provisioning_jobs");

            migrationBuilder.DropColumn(
                name: "at_schema_location",
                table: "provisioning_jobs");

            migrationBuilder.DropColumn(
                name: "at_type",
                table: "provisioning_jobs");

            migrationBuilder.DropColumn(
                name: "correlation_id",
                table: "provisioning_jobs");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "provisioning_jobs");

            migrationBuilder.DropColumn(
                name: "href",
                table: "provisioning_jobs");

            migrationBuilder.DropColumn(
                name: "priority",
                table: "provisioning_jobs");

            migrationBuilder.DropColumn(
                name: "related_party_id",
                table: "provisioning_jobs");

            migrationBuilder.DropColumn(
                name: "related_party_name",
                table: "provisioning_jobs");

            migrationBuilder.DropColumn(
                name: "related_party_role",
                table: "provisioning_jobs");
        }
    }
}
