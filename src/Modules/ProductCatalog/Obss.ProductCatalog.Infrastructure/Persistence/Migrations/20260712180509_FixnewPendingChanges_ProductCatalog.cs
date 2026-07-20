using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.ProductCatalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixnewPendingChanges_ProductCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "service_specification_id",
                table: "catalog_product_specifications",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "service_specification_id",
                table: "catalog_product_specifications");
        }
    }
}
