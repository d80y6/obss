using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Obss.CRM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Tmf629PartyRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "customers",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "href",
                table: "customers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "individual_id",
                table: "customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "organization_id",
                table: "customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status_reason",
                table: "customers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "valid_from",
                table: "customers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "valid_until",
                table: "customers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "customer_characteristics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    value_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_characteristics", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_characteristics_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer_credit_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    score_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    risk_rating = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_credit_profiles", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_credit_profiles_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer_related_parties",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    referred_id = table.Column<Guid>(type: "uuid", nullable: false),
                    referred_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_related_parties", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_related_parties_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "individuals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    salutation = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    birth_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    nationality = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    kyc_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    kyc_verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    kyc_verified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    risk_rating = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_individuals", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organizations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trading_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    company_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    industry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    registration_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tax_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    country_of_registration = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    kyc_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    kyc_verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    kyc_verified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organizations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "individual_identity_documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    individual_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    document_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    issuing_authority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    issuing_country = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    issued_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expiry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_individual_identity_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_individual_identity_documents_individuals_individual_id",
                        column: x => x.individual_id,
                        principalTable: "individuals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_customers_individual_id",
                table: "customers",
                column: "individual_id");

            migrationBuilder.CreateIndex(
                name: "ix_customers_organization_id",
                table: "customers",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_characteristics_customer_id",
                table: "customer_characteristics",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_credit_profiles_customer_id",
                table: "customer_credit_profiles",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_related_parties_customer_id",
                table: "customer_related_parties",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_individual_identity_documents_individual_id",
                table: "individual_identity_documents",
                column: "individual_id");

            migrationBuilder.AddForeignKey(
                name: "fk_customers_individuals_individual_id",
                table: "customers",
                column: "individual_id",
                principalTable: "individuals",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_customers_organizations_organization_id",
                table: "customers",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_customers_individuals_individual_id",
                table: "customers");

            migrationBuilder.DropForeignKey(
                name: "fk_customers_organizations_organization_id",
                table: "customers");

            migrationBuilder.DropTable(
                name: "customer_characteristics");

            migrationBuilder.DropTable(
                name: "customer_credit_profiles");

            migrationBuilder.DropTable(
                name: "customer_related_parties");

            migrationBuilder.DropTable(
                name: "individual_identity_documents");

            migrationBuilder.DropTable(
                name: "organizations");

            migrationBuilder.DropTable(
                name: "individuals");

            migrationBuilder.DropIndex(
                name: "ix_customers_individual_id",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "ix_customers_organization_id",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "description",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "href",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "individual_id",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "organization_id",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "status_reason",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "valid_from",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "valid_until",
                table: "customers");
        }
    }
}
