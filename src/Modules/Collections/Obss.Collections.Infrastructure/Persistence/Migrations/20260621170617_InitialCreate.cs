using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.Collections.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "collection_cases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    total_overdue_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    current_dunning_level = table.Column<int>(type: "integer", nullable: false),
                    opened_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_action_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    assigned_to = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_collection_cases", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dunning_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    max_dunning_level = table.Column<int>(type: "integer", nullable: false),
                    days_between_actions = table.Column<int>(type: "integer", nullable: false),
                    escalation_after_days = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dunning_policies", x => x.id);
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
                name: "collection_actions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    collection_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    dunning_level = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    performed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    performed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    next_action_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_collection_actions", x => x.id);
                    table.ForeignKey(
                        name: "fk_collection_actions_collection_cases_collection_case_id",
                        column: x => x.collection_case_id,
                        principalTable: "collection_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_arrangements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    collection_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    paid_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    installment_count = table.Column<int>(type: "integer", nullable: false),
                    installment_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    first_payment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_payment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    defaulted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_arrangements", x => x.id);
                    table.ForeignKey(
                        name: "fk_payment_arrangements_collection_cases_collection_case_id",
                        column: x => x.collection_case_id,
                        principalTable: "collection_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "installments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_arrangement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    installment_number = table.Column<int>(type: "integer", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    paid_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_installments", x => x.id);
                    table.ForeignKey(
                        name: "fk_installments_payment_arrangements_payment_arrangement_id",
                        column: x => x.payment_arrangement_id,
                        principalTable: "payment_arrangements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_collection_actions_action_type",
                table: "collection_actions",
                column: "action_type");

            migrationBuilder.CreateIndex(
                name: "ix_collection_actions_case_id",
                table: "collection_actions",
                column: "collection_case_id");

            migrationBuilder.CreateIndex(
                name: "ix_collection_cases_customer_id",
                table: "collection_cases",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_collection_cases_dunning_level",
                table: "collection_cases",
                column: "current_dunning_level");

            migrationBuilder.CreateIndex(
                name: "ix_collection_cases_status",
                table: "collection_cases",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_collection_cases_tenant_id",
                table: "collection_cases",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_dunning_policies_is_active",
                table: "dunning_policies",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_dunning_policies_tenant_id",
                table: "dunning_policies",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_installments_arrangement_id",
                table: "installments",
                column: "payment_arrangement_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_created_at",
                table: "outbox_messages",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_at",
                table: "outbox_messages",
                column: "processed_at");

            migrationBuilder.CreateIndex(
                name: "ix_payment_arrangements_case_id",
                table: "payment_arrangements",
                column: "collection_case_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_arrangements_status",
                table: "payment_arrangements",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "collection_actions");

            migrationBuilder.DropTable(
                name: "dunning_policies");

            migrationBuilder.DropTable(
                name: "installments");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "payment_arrangements");

            migrationBuilder.DropTable(
                name: "collection_cases");
        }
    }
}
