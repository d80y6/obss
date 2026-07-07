using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.NetworkInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OLTNetworkElementInheritance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_pon_ports_ol_ts_olt_id",
                table: "pon_ports");

            migrationBuilder.DropTable(
                name: "olts");

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "subnets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "href",
                table: "subnets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_base_type",
                table: "network_elements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_schema_location",
                table: "network_elements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_type",
                table: "network_elements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "network_elements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "discriminator",
                table: "network_elements",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "network_elements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "href",
                table: "network_elements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_bandwidth",
                table: "network_elements",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_ont_per_port",
                table: "network_elements",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_pon_ports",
                table: "network_elements",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "related_party_id",
                table: "network_elements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "related_party_name",
                table: "network_elements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "related_party_role",
                table: "network_elements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "resource_category",
                table: "network_elements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "start_date",
                table: "network_elements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "used_pon_ports",
                table: "network_elements",
                type: "integer",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_pon_ports_ol_ts_olt_id",
                table: "pon_ports",
                column: "olt_id",
                principalTable: "network_elements",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_pon_ports_ol_ts_olt_id",
                table: "pon_ports");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "subnets");

            migrationBuilder.DropColumn(
                name: "href",
                table: "subnets");

            migrationBuilder.DropColumn(
                name: "at_base_type",
                table: "network_elements");

            migrationBuilder.DropColumn(
                name: "at_schema_location",
                table: "network_elements");

            migrationBuilder.DropColumn(
                name: "at_type",
                table: "network_elements");

            migrationBuilder.DropColumn(
                name: "description",
                table: "network_elements");

            migrationBuilder.DropColumn(
                name: "discriminator",
                table: "network_elements");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "network_elements");

            migrationBuilder.DropColumn(
                name: "href",
                table: "network_elements");

            migrationBuilder.DropColumn(
                name: "max_bandwidth",
                table: "network_elements");

            migrationBuilder.DropColumn(
                name: "max_ont_per_port",
                table: "network_elements");

            migrationBuilder.DropColumn(
                name: "max_pon_ports",
                table: "network_elements");

            migrationBuilder.DropColumn(
                name: "related_party_id",
                table: "network_elements");

            migrationBuilder.DropColumn(
                name: "related_party_name",
                table: "network_elements");

            migrationBuilder.DropColumn(
                name: "related_party_role",
                table: "network_elements");

            migrationBuilder.DropColumn(
                name: "resource_category",
                table: "network_elements");

            migrationBuilder.DropColumn(
                name: "start_date",
                table: "network_elements");

            migrationBuilder.DropColumn(
                name: "used_pon_ports",
                table: "network_elements");

            migrationBuilder.CreateTable(
                name: "olts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    hostname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    max_bandwidth = table.Column<int>(type: "integer", nullable: false),
                    max_ont_per_port = table.Column<int>(type: "integer", nullable: false),
                    max_pon_ports = table.Column<int>(type: "integer", nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    software_version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    used_pon_ports = table.Column<int>(type: "integer", nullable: false),
                    vendor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_olts", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_olts_hostname",
                table: "olts",
                column: "hostname");

            migrationBuilder.AddForeignKey(
                name: "fk_pon_ports_ol_ts_olt_id",
                table: "pon_ports",
                column: "olt_id",
                principalTable: "olts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
