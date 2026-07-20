using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.Orders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameAndAddTmf622Resources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_order_fulfillments_orders_order_id",
                table: "order_fulfillments");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "order_payments");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.CreateTable(
                name: "product_order_item_relationships",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_order_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_order_item_relationships", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_order_milestones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    milestone_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_order_milestones", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    order_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    order_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    sub_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tax_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    grand_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    billing_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    billing_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    billing_state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    billing_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    billing_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    shipping_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    shipping_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    shipping_state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    shipping_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    shipping_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_by_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    approved_by_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Medium"),
                    requested_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    requested_completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expected_completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notification_contact = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    external_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    quote_id = table.Column<Guid>(type: "uuid", nullable: true),
                    href = table.Column<string>(type: "text", nullable: true),
                    at_type = table.Column<string>(type: "text", nullable: true),
                    at_base_type = table.Column<string>(type: "text", nullable: true),
                    at_schema_location = table.Column<string>(type: "text", nullable: true),
                    completion_date = table.Column<string>(type: "text", nullable: true),
                    billing_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    billing_account_href = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    order_version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    product_offering_qualification_id = table.Column<Guid>(type: "uuid", nullable: true),
                    product_offering_qualification_href = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    quote_href = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    offer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    recurring_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    billing_period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    service_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    action = table.Column<string>(type: "text", nullable: true),
                    item_state = table.Column<string>(type: "text", nullable: true),
                    state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Acknowledged")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_order_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_order_items_product_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "product_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_order_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    payment_method = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payment_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_order_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_order_payments_product_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "product_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_order_items_order_id",
                table: "product_order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_order_milestones_product_order_id",
                table: "product_order_milestones",
                column: "product_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_order_payments_order_id",
                table: "product_order_payments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_orders_customer_id",
                table: "product_orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_orders_order_date",
                table: "product_orders",
                column: "order_date");

            migrationBuilder.CreateIndex(
                name: "ix_product_orders_order_number",
                table: "product_orders",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_orders_status",
                table: "product_orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_product_orders_tenant_id",
                table: "product_orders",
                column: "tenant_id");

            migrationBuilder.AddForeignKey(
                name: "fk_order_fulfillments_product_orders_order_id",
                table: "order_fulfillments",
                column: "order_id",
                principalTable: "product_orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_order_fulfillments_product_orders_order_id",
                table: "order_fulfillments");

            migrationBuilder.DropTable(
                name: "product_order_item_relationships");

            migrationBuilder.DropTable(
                name: "product_order_items");

            migrationBuilder.DropTable(
                name: "product_order_milestones");

            migrationBuilder.DropTable(
                name: "product_order_payments");

            migrationBuilder.DropTable(
                name: "product_orders");

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_by_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_by_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    discount_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    expected_completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    external_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    grand_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    notification_contact = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    order_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    order_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    priority = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    quote_id = table.Column<Guid>(type: "uuid", nullable: true),
                    requested_completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    requested_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    sub_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tax_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    billing_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    billing_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    billing_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    billing_state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    billing_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    shipping_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    shipping_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    shipping_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    shipping_state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    shipping_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    billing_period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    recurring_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    service_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    payment_method = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payment_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_payments_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_payments_order_id",
                table: "order_payments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_customer_id",
                table: "orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_order_date",
                table: "orders",
                column: "order_date");

            migrationBuilder.CreateIndex(
                name: "ix_orders_order_number",
                table: "orders",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_status",
                table: "orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_orders_tenant_id",
                table: "orders",
                column: "tenant_id");

            migrationBuilder.AddForeignKey(
                name: "fk_order_fulfillments_orders_order_id",
                table: "order_fulfillments",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
