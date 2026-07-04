using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.ProductCatalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Tmf620ProductOfferingTerm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "ix_product_offering_terms_offer_id",
                table: "product_offering_terms",
                column: "offer_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_offering_terms_term_type",
                table: "product_offering_terms",
                column: "term_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_offering_terms");
        }
    }
}
