using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.Audit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixnewPendingChanges_Audit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No schema changes - snapshot synchronization migration
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No schema changes - snapshot synchronization migration
        }
    }
}
