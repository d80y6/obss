using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.ProductCatalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Tmf620PricingEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "offer_pricings",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "offer_pricings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "price_application_type",
                table: "offer_pricings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

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
                name: "ix_price_ranges_offer_pricing_id",
                table: "price_ranges",
                column: "offer_pricing_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "price_ranges");

            migrationBuilder.DropColumn(
                name: "description",
                table: "offer_pricings");

            migrationBuilder.DropColumn(
                name: "name",
                table: "offer_pricings");

            migrationBuilder.DropColumn(
                name: "price_application_type",
                table: "offer_pricings");
        }
    }
}
