using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.ServiceInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixnewPendingChanges_ServiceInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "at_base_type",
                table: "services",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_schema_location",
                table: "services",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "at_type",
                table: "services",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "services",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "completion_date",
                table: "services",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "services",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "services",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "href",
                table: "services",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "services",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "place",
                table: "services",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "related_party_id",
                table: "services",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "related_party_name",
                table: "services",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "related_party_role",
                table: "services",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "service_specification_id",
                table: "services",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "start_date",
                table: "services",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "at_base_type",
                table: "services");

            migrationBuilder.DropColumn(
                name: "at_schema_location",
                table: "services");

            migrationBuilder.DropColumn(
                name: "at_type",
                table: "services");

            migrationBuilder.DropColumn(
                name: "category",
                table: "services");

            migrationBuilder.DropColumn(
                name: "completion_date",
                table: "services");

            migrationBuilder.DropColumn(
                name: "description",
                table: "services");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "services");

            migrationBuilder.DropColumn(
                name: "href",
                table: "services");

            migrationBuilder.DropColumn(
                name: "name",
                table: "services");

            migrationBuilder.DropColumn(
                name: "place",
                table: "services");

            migrationBuilder.DropColumn(
                name: "related_party_id",
                table: "services");

            migrationBuilder.DropColumn(
                name: "related_party_name",
                table: "services");

            migrationBuilder.DropColumn(
                name: "related_party_role",
                table: "services");

            migrationBuilder.DropColumn(
                name: "service_specification_id",
                table: "services");

            migrationBuilder.DropColumn(
                name: "start_date",
                table: "services");
        }
    }
}
