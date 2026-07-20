using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Obss.ProductCatalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    parent_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "offers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    offer_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_contract = table.Column<bool>(type: "boolean", nullable: false),
                    contract_duration_months = table.Column<int>(type: "integer", nullable: true),
                    billing_period = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    tax_inclusive = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offers", x => x.id);
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
                name: "product_configuration_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    target_product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    target_option = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    condition = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_configuration_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_options",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    option_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    is_multi_select = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_options", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    product_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_shippable = table.Column<bool>(type: "boolean", nullable: false),
                    taxable = table.Column<bool>(type: "boolean", nullable: false),
                    tax_category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    lifecycle_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "offer_discounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    discount_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    discount_value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount_period_months = table.Column<int>(type: "integer", nullable: true),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_discounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_offer_discounts_offers_offer_id",
                        column: x => x.offer_id,
                        principalTable: "offers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "offer_pricings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pricing_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    recurring_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    one_time_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    usage_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    unit_of_measure = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    min_quantity = table.Column<int>(type: "integer", nullable: true),
                    max_quantity = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offer_pricings", x => x.id);
                    table.ForeignKey(
                        name: "fk_offer_pricings_offers_offer_id",
                        column: x => x.offer_id,
                        principalTable: "offers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "option_values",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_option_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    price_adjustment = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_option_values", x => x.id);
                    table.ForeignKey(
                        name: "fk_option_values_product_options_product_option_id",
                        column: x => x.product_option_id,
                        principalTable: "product_options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_offers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_offers", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_offers_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_specifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_specifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_specifications_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_categories_parent_category_id",
                table: "categories",
                column: "parent_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_tenant_id",
                table: "categories",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_tenant_id_name",
                table: "categories",
                columns: new[] { "tenant_id", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_offer_discounts_offer_id",
                table: "offer_discounts",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_offer_pricings_offer_id",
                table: "offer_pricings",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_offers_is_active",
                table: "offers",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_offers_offer_type",
                table: "offers",
                column: "offer_type");

            migrationBuilder.CreateIndex(
                name: "ix_offers_tenant_id",
                table: "offers",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_offers_tenant_id_name",
                table: "offers",
                columns: new[] { "tenant_id", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_option_values_product_option_id",
                table: "option_values",
                column: "product_option_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_created_at",
                table: "outbox_messages",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_at",
                table: "outbox_messages",
                column: "processed_at");

            migrationBuilder.CreateIndex(
                name: "ix_product_configuration_rules_product_id",
                table: "product_configuration_rules",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_configuration_rules_rule_type",
                table: "product_configuration_rules",
                column: "rule_type");

            migrationBuilder.CreateIndex(
                name: "ix_product_offers_offer_id",
                table: "product_offers",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_offers_product_id",
                table: "product_offers",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_offers_product_offer",
                table: "product_offers",
                columns: new[] { "product_id", "offer_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_options_option_type",
                table: "product_options",
                column: "option_type");

            migrationBuilder.CreateIndex(
                name: "ix_product_options_product_id",
                table: "product_options",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_specifications_product_id",
                table: "product_specifications",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_lifecycle_status",
                table: "products",
                column: "lifecycle_status");

            migrationBuilder.CreateIndex(
                name: "ix_products_product_type",
                table: "products",
                column: "product_type");

            migrationBuilder.CreateIndex(
                name: "ix_products_tenant_id",
                table: "products",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_tenant_id_name",
                table: "products",
                columns: new[] { "tenant_id", "name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "offer_discounts");

            migrationBuilder.DropTable(
                name: "offer_pricings");

            migrationBuilder.DropTable(
                name: "option_values");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "product_configuration_rules");

            migrationBuilder.DropTable(
                name: "product_offers");

            migrationBuilder.DropTable(
                name: "product_specifications");

            migrationBuilder.DropTable(
                name: "offers");

            migrationBuilder.DropTable(
                name: "product_options");

            migrationBuilder.DropTable(
                name: "products");
        }
    }
}
