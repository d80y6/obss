using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.Billing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNoBillingTmplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "billing_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    total_processed = table.Column<int>(type: "integer", nullable: false),
                    total_errors = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_billing_jobs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_billing_jobs_created_at",
                table: "billing_jobs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_billing_jobs_status",
                table: "billing_jobs",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "billing_jobs");
        }
    }
}
