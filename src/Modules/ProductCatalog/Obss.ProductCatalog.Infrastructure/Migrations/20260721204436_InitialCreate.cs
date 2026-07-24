using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Obss.ProductCatalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "catalog_product_specifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    brand = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    product_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    lifecycle_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    service_specification_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_catalog_product_specifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "catalogs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    catalog_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    lifecycle_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_catalogs", x => x.id);
                });

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
                    lifecycle_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

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
                    correlation_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    last_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    next_attempt_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_dead_lettered = table.Column<bool>(type: "boolean", nullable: false),
                    lock_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lock_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                name: "product_specification_characteristics",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_specification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    value_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    configurable = table.Column<bool>(type: "boolean", nullable: false),
                    min_value = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    max_value = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    regex = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    max_cardinality = table.Column<int>(type: "integer", nullable: true),
                    is_required = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_specification_characteristics", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_specification_characteristics_product_specification",
                        column: x => x.product_specification_id,
                        principalTable: "catalog_product_specifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_specification_relationships",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_specification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_specification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    role = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_specification_relationships", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_specification_relationships_catalog_product_specifi",
                        column: x => x.product_specification_id,
                        principalTable: "catalog_product_specifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    product_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    product_specification_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lifecycle_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                    table.ForeignKey(
                        name: "fk_products_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_products_product_specifications_product_specification_id",
                        column: x => x.product_specification_id,
                        principalTable: "catalog_product_specifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "bundled_product_offerings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bundled_offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    referral_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bundled_product_offerings", x => x.id);
                    table.ForeignKey(
                        name: "fk_bundled_product_offerings_offers_bundled_offer_id",
                        column: x => x.bundled_offer_id,
                        principalTable: "offers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_bundled_product_offerings_offers_offer_id",
                        column: x => x.offer_id,
                        principalTable: "offers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    price_application_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                name: "product_offering_terms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    duration = table.Column<int>(type: "integer", nullable: false),
                    duration_unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    term_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_offering_terms", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_offering_terms_offers_offer_id",
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
                name: "product_specification_characteristic_values",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    characteristic_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    unit_of_measure = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    value_from = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    value_to = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    range_interval = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_specification_characteristic_values", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_specification_characteristic_values_product_specifi",
                        column: x => x.characteristic_id,
                        principalTable: "product_specification_characteristics",
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
                        name: "fk_product_offers_offers_offer_id",
                        column: x => x.offer_id,
                        principalTable: "offers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "price_ranges",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    offer_pricing_id = table.Column<Guid>(type: "uuid", nullable: false),
                    min_quantity = table.Column<int>(type: "integer", nullable: false),
                    max_quantity = table.Column<int>(type: "integer", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_price_ranges", x => x.id);
                    table.ForeignKey(
                        name: "fk_price_ranges_offer_pricings_offer_pricing_id",
                        column: x => x.offer_pricing_id,
                        principalTable: "offer_pricings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bundled_product_offerings_bundled_offer_id",
                table: "bundled_product_offerings",
                column: "bundled_offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_bundled_product_offerings_offer_bundled",
                table: "bundled_product_offerings",
                columns: new[] { "offer_id", "bundled_offer_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_bundled_product_offerings_offer_id",
                table: "bundled_product_offerings",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_specifications_lifecycle_status",
                table: "catalog_product_specifications",
                column: "lifecycle_status");

            migrationBuilder.CreateIndex(
                name: "ix_product_specifications_name",
                table: "catalog_product_specifications",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_product_specifications_tenant_id",
                table: "catalog_product_specifications",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_specifications_tenant_id_product_number",
                table: "catalog_product_specifications",
                columns: new[] { "tenant_id", "product_number" },
                unique: true,
                filter: "\"product_number\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_catalogs_catalog_type",
                table: "catalogs",
                column: "catalog_type");

            migrationBuilder.CreateIndex(
                name: "ix_catalogs_lifecycle_status",
                table: "catalogs",
                column: "lifecycle_status");

            migrationBuilder.CreateIndex(
                name: "ix_catalogs_name",
                table: "catalogs",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_catalogs_tenant_id",
                table: "catalogs",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_catalogs_tenant_id_name",
                table: "catalogs",
                columns: new[] { "tenant_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_lifecycle_status",
                table: "categories",
                column: "lifecycle_status");

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
                name: "ix_inbox_messages_event_id_handler_name",
                table: "inbox_messages",
                columns: new[] { "event_id", "handler_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inbox_messages_received_at",
                table: "inbox_messages",
                column: "received_at");

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
                name: "ix_outbox_messages_pending",
                table: "outbox_messages",
                columns: new[] { "processed_at", "next_attempt_at", "is_dead_lettered" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_at",
                table: "outbox_messages",
                column: "processed_at");

            migrationBuilder.CreateIndex(
                name: "ix_price_ranges_offer_pricing_id",
                table: "price_ranges",
                column: "offer_pricing_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_configuration_rules_product_id",
                table: "product_configuration_rules",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_configuration_rules_rule_type",
                table: "product_configuration_rules",
                column: "rule_type");

            migrationBuilder.CreateIndex(
                name: "ix_product_offering_terms_offer_id",
                table: "product_offering_terms",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_offering_terms_term_type",
                table: "product_offering_terms",
                column: "term_type");

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
                name: "ix_spec_char_values_characteristic_id",
                table: "product_specification_characteristic_values",
                column: "characteristic_id");

            migrationBuilder.CreateIndex(
                name: "ix_spec_characteristics_product_specification_id",
                table: "product_specification_characteristics",
                column: "product_specification_id");

            migrationBuilder.CreateIndex(
                name: "ix_spec_relationships_product_specification_id",
                table: "product_specification_relationships",
                column: "product_specification_id");

            migrationBuilder.CreateIndex(
                name: "ix_spec_relationships_target_specification_id",
                table: "product_specification_relationships",
                column: "target_specification_id");

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
                name: "ix_products_product_specification_id",
                table: "products",
                column: "product_specification_id");

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
                name: "bundled_product_offerings");

            migrationBuilder.DropTable(
                name: "catalogs");

            migrationBuilder.DropTable(
                name: "inbox_messages");

            migrationBuilder.DropTable(
                name: "offer_discounts");

            migrationBuilder.DropTable(
                name: "option_values");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "price_ranges");

            migrationBuilder.DropTable(
                name: "product_configuration_rules");

            migrationBuilder.DropTable(
                name: "product_offering_terms");

            migrationBuilder.DropTable(
                name: "product_offers");

            migrationBuilder.DropTable(
                name: "product_specification_characteristic_values");

            migrationBuilder.DropTable(
                name: "product_specification_relationships");

            migrationBuilder.DropTable(
                name: "product_specifications");

            migrationBuilder.DropTable(
                name: "product_options");

            migrationBuilder.DropTable(
                name: "offer_pricings");

            migrationBuilder.DropTable(
                name: "product_specification_characteristics");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "offers");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "catalog_product_specifications");
        }
    }
}
