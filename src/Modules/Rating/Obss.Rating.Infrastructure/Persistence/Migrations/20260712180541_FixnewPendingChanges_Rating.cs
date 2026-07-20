using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Obss.Rating.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixnewPendingChanges_Rating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "at_base_type",
                table: "usage_records",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_schema_location",
                table: "usage_records",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_type",
                table: "usage_records",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "usage_records",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "href",
                table: "usage_records",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "rating_rules",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "promotions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "usage_record_related_parties",
                columns: table => new
                {
                    usage_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    party_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    party_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usage_record_related_parties", x => new { x.usage_record_id, x.id });
                    table.ForeignKey(
                        name: "fk_usage_record_related_parties_usage_records_usage_record_id",
                        column: x => x.usage_record_id,
                        principalTable: "usage_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usage_record_related_parties");

            migrationBuilder.DropColumn(
                name: "at_base_type",
                table: "usage_records");

            migrationBuilder.DropColumn(
                name: "at_schema_location",
                table: "usage_records");

            migrationBuilder.DropColumn(
                name: "at_type",
                table: "usage_records");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "usage_records");

            migrationBuilder.DropColumn(
                name: "href",
                table: "usage_records");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "rating_rules");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "promotions");
        }
    }
}
