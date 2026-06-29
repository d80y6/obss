using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.Billing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "billing_cycles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    billing_period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    last_billing_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    next_billing_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_billing_cycles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bills",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    billing_period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    billing_period_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    billing_period_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sub_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tax_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    grand_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    finalized_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bills", x => x.id);
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
                name: "tax_exemptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tax_rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exemption_certificate = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    exemption_rate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    approved_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tax_exemptions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tax_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    tax_rate = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    tax_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tax_category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    country = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "YE"),
                    region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_compound = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    effective_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    effective_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tax_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bill_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    subscription_id = table.Column<Guid>(type: "uuid", nullable: true),
                    product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tax_rate = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    line_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bill_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_bill_lines_bills_bill_id",
                        column: x => x.bill_id,
                        principalTable: "bills",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bill_lines_bill_id",
                table: "bill_lines",
                column: "bill_id");

            migrationBuilder.CreateIndex(
                name: "ix_billing_cycles_customer_id",
                table: "billing_cycles",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_billing_cycles_next_billing_date",
                table: "billing_cycles",
                column: "next_billing_date");

            migrationBuilder.CreateIndex(
                name: "ix_bills_customer_id",
                table: "bills",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_bills_customer_id_status",
                table: "bills",
                columns: new[] { "customer_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_bills_status",
                table: "bills",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_bills_tenant_id",
                table: "bills",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_created_at",
                table: "outbox_messages",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_at",
                table: "outbox_messages",
                column: "processed_at");

            migrationBuilder.CreateIndex(
                name: "ix_tax_exemptions_customer_id",
                table: "tax_exemptions",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_tax_exemptions_customer_tax_rule",
                table: "tax_exemptions",
                columns: new[] { "customer_id", "tax_rule_id" });

            migrationBuilder.CreateIndex(
                name: "ix_tax_rules_category_country",
                table: "tax_rules",
                columns: new[] { "tax_category", "country" });

            migrationBuilder.CreateIndex(
                name: "ix_tax_rules_is_active",
                table: "tax_rules",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_tax_rules_tenant_id",
                table: "tax_rules",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bill_lines");

            migrationBuilder.DropTable(
                name: "billing_cycles");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "tax_exemptions");

            migrationBuilder.DropTable(
                name: "tax_rules");

            migrationBuilder.DropTable(
                name: "bills");
        }
    }
}
