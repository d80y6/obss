using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Obss.Payments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixnewPendingChanges_Payments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "refunds",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_base_type",
                table: "payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "at_schema_location",
                table: "payments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_type",
                table: "payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "href",
                table: "payments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "payment_allocations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "payment_related_parties",
                columns: table => new
                {
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    party_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    party_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_related_parties", x => new { x.payment_id, x.id });
                    table.ForeignKey(
                        name: "fk_payment_related_parties_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_related_parties");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "refunds");

            migrationBuilder.DropColumn(
                name: "at_base_type",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "at_schema_location",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "at_type",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "href",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "payment_allocations");
        }
    }
}
