using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Obss.Invoices.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixnewPendingChanges_Invoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "at_base_type",
                table: "invoices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_schema_location",
                table: "invoices",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_type",
                table: "invoices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "invoices",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "href",
                table: "invoices",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "invoice_lines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "credit_notes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "credit_note_lines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "invoice_related_parties",
                columns: table => new
                {
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    party_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    party_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoice_related_parties", x => new { x.invoice_id, x.id });
                    table.ForeignKey(
                        name: "fk_invoice_related_parties_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invoice_related_parties");

            migrationBuilder.DropColumn(
                name: "at_base_type",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "at_schema_location",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "at_type",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "href",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "invoice_lines");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "credit_notes");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "credit_note_lines");
        }
    }
}
