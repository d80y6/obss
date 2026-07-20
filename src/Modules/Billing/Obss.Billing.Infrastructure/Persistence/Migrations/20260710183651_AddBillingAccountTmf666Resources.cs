using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Obss.Billing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingAccountTmf666Resources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "tax_rules",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "billing_account_id",
                table: "tax_exemptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "tax_exemptions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_base_type",
                table: "bills",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_schema_location",
                table: "bills",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_type",
                table: "bills",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "bills",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "href",
                table: "bills",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "billing_cycles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "account_holder_contact_id",
                table: "billing_accounts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "account_holder_email",
                table: "billing_accounts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "account_holder_name",
                table: "billing_accounts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "account_holder_phone",
                table: "billing_accounts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_base_type",
                table: "billing_accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_schema_location",
                table: "billing_accounts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_type",
                table: "billing_accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "billing_accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "href",
                table: "billing_accounts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_method_id",
                table: "billing_accounts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "bill_lines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "account_balances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    billing_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    outstanding_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    available_credit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    balance_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    at_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    at_base_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    at_schema_location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account_balances", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bill_related_parties",
                columns: table => new
                {
                    bill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    party_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    party_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bill_related_parties", x => new { x.bill_id, x.id });
                    table.ForeignKey(
                        name: "fk_bill_related_parties_bills_bill_id",
                        column: x => x.bill_id,
                        principalTable: "bills",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "billing_account_presentation_media",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    media_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    paper_format = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    is_preferred = table.Column<bool>(type: "boolean", nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    billing_account_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_billing_account_presentation_media", x => x.id);
                    table.ForeignKey(
                        name: "fk_billing_account_presentation_media_billing_accounts_billing",
                        column: x => x.billing_account_id,
                        principalTable: "billing_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "billing_account_related_parties",
                columns: table => new
                {
                    billing_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    party_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    party_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_billing_account_related_parties", x => new { x.billing_account_id, x.id });
                    table.ForeignKey(
                        name: "fk_billing_account_related_parties_billing_accounts_billing_ac",
                        column: x => x.billing_account_id,
                        principalTable: "billing_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "balance_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    balance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    transaction_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reference_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reference_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    balance_id1 = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_balance_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_balance_transactions_account_balances_balance_id",
                        column: x => x.balance_id1,
                        principalTable: "account_balances",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_account_balances_balance_date",
                table: "account_balances",
                column: "balance_date");

            migrationBuilder.CreateIndex(
                name: "ix_account_balances_billing_account_id",
                table: "account_balances",
                column: "billing_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_balance_transactions_balance_id",
                table: "balance_transactions",
                column: "balance_id1");

            migrationBuilder.CreateIndex(
                name: "ix_billing_account_presentation_media_billing_account_id",
                table: "billing_account_presentation_media",
                column: "billing_account_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "balance_transactions");

            migrationBuilder.DropTable(
                name: "bill_related_parties");

            migrationBuilder.DropTable(
                name: "billing_account_presentation_media");

            migrationBuilder.DropTable(
                name: "billing_account_related_parties");

            migrationBuilder.DropTable(
                name: "account_balances");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "tax_rules");

            migrationBuilder.DropColumn(
                name: "billing_account_id",
                table: "tax_exemptions");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "tax_exemptions");

            migrationBuilder.DropColumn(
                name: "at_base_type",
                table: "bills");

            migrationBuilder.DropColumn(
                name: "at_schema_location",
                table: "bills");

            migrationBuilder.DropColumn(
                name: "at_type",
                table: "bills");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "bills");

            migrationBuilder.DropColumn(
                name: "href",
                table: "bills");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "billing_cycles");

            migrationBuilder.DropColumn(
                name: "account_holder_contact_id",
                table: "billing_accounts");

            migrationBuilder.DropColumn(
                name: "account_holder_email",
                table: "billing_accounts");

            migrationBuilder.DropColumn(
                name: "account_holder_name",
                table: "billing_accounts");

            migrationBuilder.DropColumn(
                name: "account_holder_phone",
                table: "billing_accounts");

            migrationBuilder.DropColumn(
                name: "at_base_type",
                table: "billing_accounts");

            migrationBuilder.DropColumn(
                name: "at_schema_location",
                table: "billing_accounts");

            migrationBuilder.DropColumn(
                name: "at_type",
                table: "billing_accounts");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "billing_accounts");

            migrationBuilder.DropColumn(
                name: "href",
                table: "billing_accounts");

            migrationBuilder.DropColumn(
                name: "payment_method_id",
                table: "billing_accounts");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "bill_lines");
        }
    }
}
