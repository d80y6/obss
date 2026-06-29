using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.ServiceInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "resource_discovery_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    discovery_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    configuration = table.Column<string>(type: "jsonb", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resources_found = table.Column<int>(type: "integer", nullable: false),
                    resources_matched = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resource_discovery_jobs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "service_topologies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    topology_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_topologies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "services",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    service_identifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    activation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    suspended_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    decommissioned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    configuration = table.Column<string>(type: "jsonb", nullable: true),
                    location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_services", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "topology_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_topology_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    link_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    direction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    attributes = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_topology_links", x => x.id);
                    table.ForeignKey(
                        name: "fk_topology_links_service_topologies_service_topology_id",
                        column: x => x.service_topology_id,
                        principalTable: "service_topologies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "service_resources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    resource_identifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    allocated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    released_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_resources", x => x.id);
                    table.ForeignKey(
                        name: "fk_service_resources_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_created_at",
                table: "outbox_messages",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_at",
                table: "outbox_messages",
                column: "processed_at");

            migrationBuilder.CreateIndex(
                name: "ix_resource_discovery_jobs_status",
                table: "resource_discovery_jobs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_resource_discovery_jobs_tenant_id",
                table: "resource_discovery_jobs",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_resources_resource_identifier",
                table: "service_resources",
                column: "resource_identifier");

            migrationBuilder.CreateIndex(
                name: "ix_service_resources_service_id",
                table: "service_resources",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_topologies_service_id",
                table: "service_topologies",
                column: "service_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_services_customer_id",
                table: "services",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_services_customer_id_status",
                table: "services",
                columns: new[] { "customer_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_services_service_identifier",
                table: "services",
                column: "service_identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_services_service_type",
                table: "services",
                column: "service_type");

            migrationBuilder.CreateIndex(
                name: "ix_services_status",
                table: "services",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_services_subscription_id",
                table: "services",
                column: "subscription_id");

            migrationBuilder.CreateIndex(
                name: "ix_topology_links_service_topology_id",
                table: "topology_links",
                column: "service_topology_id");

            migrationBuilder.CreateIndex(
                name: "ix_topology_links_source_service_id",
                table: "topology_links",
                column: "source_service_id");

            migrationBuilder.CreateIndex(
                name: "ix_topology_links_target_service_id",
                table: "topology_links",
                column: "target_service_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "resource_discovery_jobs");

            migrationBuilder.DropTable(
                name: "service_resources");

            migrationBuilder.DropTable(
                name: "topology_links");

            migrationBuilder.DropTable(
                name: "services");

            migrationBuilder.DropTable(
                name: "service_topologies");
        }
    }
}
