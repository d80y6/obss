using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.Orders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewPendingChanges_Orders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "channel",
                table: "orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "orders",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "expected_completion_date",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notification_contact",
                table: "orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "priority",
                table: "orders",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "quote_id",
                table: "orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "requested_completion_date",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "requested_start_date",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "channel",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "description",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "expected_completion_date",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "notification_contact",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "priority",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "quote_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "requested_completion_date",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "requested_start_date",
                table: "orders");
        }
    }
}
