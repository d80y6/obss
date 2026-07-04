using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.ProductCatalog.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogAndCategoryAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "lifecycle_status",
                table: "categories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "categories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "valid_from",
                table: "categories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "valid_to",
                table: "categories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "version",
                table: "categories",
                type: "integer",
                nullable: false,
                defaultValue: 1);

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

            migrationBuilder.CreateIndex(
                name: "ix_categories_lifecycle_status",
                table: "categories",
                column: "lifecycle_status");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "catalogs");

            migrationBuilder.DropIndex(
                name: "ix_categories_lifecycle_status",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "lifecycle_status",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "valid_from",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "valid_to",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "version",
                table: "categories");
        }
    }
}
