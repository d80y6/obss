using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Obss.ServiceQualification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceQualificationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "coverage_areas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    street_from = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    street_to = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_coverage_areas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    handler_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    event_type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    event_data = table.Column<string>(type: "jsonb", nullable: false),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    event_data = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "service_qualifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    address_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address_state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    address_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address_country = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    requested_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expiration_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_qualifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "coverage_area_services",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    speed_mbps = table.Column<int>(type: "integer", nullable: true),
                    technology = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    monthly_price = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    coverage_area_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_coverage_area_services", x => x.id);
                    table.ForeignKey(
                        name: "fk_coverage_area_services_coverage_areas_coverage_area_id",
                        column: x => x.coverage_area_id,
                        principalTable: "coverage_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_qualification_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    result_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    estimated_install_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    estimated_completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    eligibility_unavailable_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    qualification_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_qualification_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_qualification_items_service_qualifications_qualific",
                        column: x => x.qualification_id,
                        principalTable: "service_qualifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_qualification_item_alternatives",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    result_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    estimated_install_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    guaranteed_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    item_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_qualification_item_alternatives", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_qualification_item_alternatives_service_qualificati",
                        column: x => x.item_id,
                        principalTable: "service_qualification_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "coverage_areas",
                columns: new[] { "id", "city", "postal_code", "state", "street_from", "street_to" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "Sana'a", null, null, null, null },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "Aden", null, null, null, null },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "Taiz", null, null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_coverage_area_services_coverage_area_id",
                table: "coverage_area_services",
                column: "coverage_area_id");

            migrationBuilder.CreateIndex(
                name: "ix_inbox_messages_event_id_handler_name",
                table: "inbox_messages",
                columns: new[] { "event_id", "handler_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inbox_messages_received_at",
                table: "inbox_messages",
                column: "received_at");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_created_at",
                table: "outbox_messages",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_at",
                table: "outbox_messages",
                column: "processed_at");

            migrationBuilder.CreateIndex(
                name: "ix_service_qualification_item_alternatives_item_id",
                table: "service_qualification_item_alternatives",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_qualification_items_qualification_id",
                table: "service_qualification_items",
                column: "qualification_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "coverage_area_services");

            migrationBuilder.DropTable(
                name: "inbox_messages");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "service_qualification_item_alternatives");

            migrationBuilder.DropTable(
                name: "coverage_areas");

            migrationBuilder.DropTable(
                name: "service_qualification_items");

            migrationBuilder.DropTable(
                name: "service_qualifications");
        }
    }
}
