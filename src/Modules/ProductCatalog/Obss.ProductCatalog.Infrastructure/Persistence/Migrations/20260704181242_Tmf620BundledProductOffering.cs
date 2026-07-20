using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.ProductCatalog.Infrastructure.Persistence.Migrations
{
    public partial class Tmf620BundledProductOffering : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bundled_product_offerings");
        }
    }
}
