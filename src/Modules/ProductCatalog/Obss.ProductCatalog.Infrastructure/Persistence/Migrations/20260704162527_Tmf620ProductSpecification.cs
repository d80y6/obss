using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.ProductCatalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Tmf620ProductSpecification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "product_number",
                table: "products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "product_specification_id",
                table: "products",
                type: "uuid",
                nullable: true);

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
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_catalog_product_specifications", x => x.id);
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

            migrationBuilder.CreateIndex(
                name: "ix_products_product_specification_id",
                table: "products",
                column: "product_specification_id");

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

            migrationBuilder.AddForeignKey(
                name: "fk_products_product_specifications_product_specification_id",
                table: "products",
                column: "product_specification_id",
                principalTable: "catalog_product_specifications",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_products_product_specifications_product_specification_id",
                table: "products");

            migrationBuilder.DropTable(
                name: "product_specification_characteristic_values");

            migrationBuilder.DropTable(
                name: "product_specification_relationships");

            migrationBuilder.DropTable(
                name: "product_specification_characteristics");

            migrationBuilder.DropTable(
                name: "catalog_product_specifications");

            migrationBuilder.DropIndex(
                name: "ix_products_product_specification_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "product_number",
                table: "products");

            migrationBuilder.DropColumn(
                name: "product_specification_id",
                table: "products");
        }
    }
}
