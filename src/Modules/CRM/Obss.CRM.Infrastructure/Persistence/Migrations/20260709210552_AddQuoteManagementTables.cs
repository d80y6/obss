using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.CRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteManagementTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_customer_related_parties",
                table: "customer_related_parties");

            migrationBuilder.DropPrimaryKey(
                name: "pk_customer_agreement_refs",
                table: "customer_agreement_refs");

            migrationBuilder.DropPrimaryKey(
                name: "pk_customer_account_refs",
                table: "customer_account_refs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_customer_related_parties",
                table: "customer_related_parties",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_customer_agreement_refs",
                table: "customer_agreement_refs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_customer_account_refs",
                table: "customer_account_refs",
                column: "id");

            migrationBuilder.CreateTable(
                name: "quotes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    external_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    quote_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expected_quote_completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    effective_quote_completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expected_fulfillment_start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    agreement_refs = table.Column<string>(type: "jsonb", nullable: true),
                    billing_account_refs = table.Column<string>(type: "jsonb", nullable: true),
                    notes = table.Column<string>(type: "jsonb", nullable: true),
                    quote_authorizations = table.Column<string>(type: "jsonb", nullable: true),
                    quote_prices = table.Column<string>(type: "jsonb", nullable: true),
                    related_parties = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quotes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "quote_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    product_offering_id = table.Column<Guid>(type: "uuid", nullable: true),
                    product_offering_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    quote_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_relationships = table.Column<string>(type: "jsonb", nullable: true),
                    notes = table.Column<string>(type: "jsonb", nullable: true),
                    prices = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quote_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_quote_items_quotes_quote_id",
                        column: x => x.quote_id,
                        principalTable: "quotes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_quote_items_quote_id",
                table: "quote_items",
                column: "quote_id");

            migrationBuilder.CreateIndex(
                name: "ix_quotes_customer_id",
                table: "quotes",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_quotes_state",
                table: "quotes",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "ix_quotes_tenant_id",
                table: "quotes",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quote_items");

            migrationBuilder.DropTable(
                name: "quotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_customer_related_parties",
                table: "customer_related_parties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_customer_agreement_refs",
                table: "customer_agreement_refs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_customer_account_refs",
                table: "customer_account_refs");

            migrationBuilder.AddPrimaryKey(
                name: "pk_customer_related_parties",
                table: "customer_related_parties",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_customer_agreement_refs",
                table: "customer_agreement_refs",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_customer_account_refs",
                table: "customer_account_refs",
                column: "id");
        }
    }
}
