using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Obss.CRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SubProject2ContactMedium : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customer_contact_media",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    medium_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_preferred = table.Column<bool>(type: "boolean", nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_contact_media", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_contact_media_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer_contact_medium_characteristics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    value_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    contact_medium_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_contact_medium_characteristics", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_contact_medium_characteristics_customer_contact_me",
                        column: x => x.contact_medium_id,
                        principalTable: "customer_contact_media",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_customer_contact_media_customer_id",
                table: "customer_contact_media",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_contact_medium_characteristics_contact_medium_id",
                table: "customer_contact_medium_characteristics",
                column: "contact_medium_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_contact_medium_characteristics");

            migrationBuilder.DropTable(
                name: "customer_contact_media");
        }
    }
}
