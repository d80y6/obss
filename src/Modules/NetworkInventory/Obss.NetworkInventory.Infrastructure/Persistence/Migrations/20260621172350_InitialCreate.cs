using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Obss.NetworkInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "network_elements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    hostname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    element_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    vendor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    software_version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    management_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    snmp_community = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_managed = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_network_elements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "olts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    hostname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    vendor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    software_version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    max_pon_ports = table.Column<int>(type: "integer", nullable: false),
                    used_pon_ports = table.Column<int>(type: "integer", nullable: false),
                    max_ont_per_port = table.Column<int>(type: "integer", nullable: false),
                    max_bandwidth = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_olts", x => x.id);
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
                name: "subnets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    network = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    gateway = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    vlan_id = table.Column<int>(type: "integer", nullable: false),
                    location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subnets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "topology_maps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    configuration = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_topology_maps", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vlans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    vlan_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vlans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "capacity_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    element_id = table.Column<Guid>(type: "uuid", nullable: false),
                    interface_id = table.Column<Guid>(type: "uuid", nullable: true),
                    capacity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    total_capacity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    used_capacity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    available_capacity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    utilization_percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    measured_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_capacity_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_capacity_records_network_elements_element_id",
                        column: x => x.element_id,
                        principalTable: "network_elements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "connectivity_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    source_element_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_interface_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_element_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_interface_id = table.Column<Guid>(type: "uuid", nullable: false),
                    link_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    bandwidth = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    protocol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    latency_ms = table.Column<int>(type: "integer", nullable: false),
                    mtu = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connectivity_links", x => x.id);
                    table.ForeignKey(
                        name: "fk_connectivity_links_network_elements_source_element_id",
                        column: x => x.source_element_id,
                        principalTable: "network_elements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_connectivity_links_network_elements_target_element_id",
                        column: x => x.target_element_id,
                        principalTable: "network_elements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fiber_cables",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_element_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_interface_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_element_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_interface_id = table.Column<Guid>(type: "uuid", nullable: false),
                    length = table.Column<int>(type: "integer", nullable: false),
                    fiber_count = table.Column<int>(type: "integer", nullable: false),
                    fiber_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    splicing_points = table.Column<int>(type: "integer", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fiber_cables", x => x.id);
                    table.ForeignKey(
                        name: "fk_fiber_cables_network_elements_from_element_id",
                        column: x => x.from_element_id,
                        principalTable: "network_elements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_fiber_cables_network_elements_to_element_id",
                        column: x => x.to_element_id,
                        principalTable: "network_elements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "network_element_ip_addresses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    network_element_id = table.Column<Guid>(type: "uuid", nullable: false),
                    network_interface_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    subnet_mask = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    gateway = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    address_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_available = table.Column<bool>(type: "boolean", nullable: false),
                    is_reserved = table.Column<bool>(type: "boolean", nullable: false),
                    assigned_to = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_network_element_ip_addresses", x => x.id);
                    table.ForeignKey(
                        name: "fk_network_element_ip_addresses_network_elements_network_eleme",
                        column: x => x.network_element_id,
                        principalTable: "network_elements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "network_interfaces",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    network_element_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    interface_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    speed = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    mac_address = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: true),
                    mtu = table.Column<int>(type: "integer", nullable: false),
                    connected_to_interface_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_network_interfaces", x => x.id);
                    table.ForeignKey(
                        name: "fk_network_interfaces_network_elements_network_element_id",
                        column: x => x.network_element_id,
                        principalTable: "network_elements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pon_ports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    olt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    port_number = table.Column<int>(type: "integer", nullable: false),
                    port_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    max_ont = table.Column<int>(type: "integer", nullable: false),
                    connected_ont_count = table.Column<int>(type: "integer", nullable: false),
                    max_distance = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pon_ports", x => x.id);
                    table.ForeignKey(
                        name: "fk_pon_ports_ol_ts_olt_id",
                        column: x => x.olt_id,
                        principalTable: "olts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_capacity_records_element_id",
                table: "capacity_records",
                column: "element_id");

            migrationBuilder.CreateIndex(
                name: "ix_capacity_records_utilization_percent",
                table: "capacity_records",
                column: "utilization_percent");

            migrationBuilder.CreateIndex(
                name: "ix_connectivity_links_source_element_id",
                table: "connectivity_links",
                column: "source_element_id");

            migrationBuilder.CreateIndex(
                name: "ix_connectivity_links_status",
                table: "connectivity_links",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_connectivity_links_target_element_id",
                table: "connectivity_links",
                column: "target_element_id");

            migrationBuilder.CreateIndex(
                name: "ix_fiber_cables_from_element_id",
                table: "fiber_cables",
                column: "from_element_id");

            migrationBuilder.CreateIndex(
                name: "ix_fiber_cables_to_element_id",
                table: "fiber_cables",
                column: "to_element_id");

            migrationBuilder.CreateIndex(
                name: "ix_network_element_ip_addresses_ip_address",
                table: "network_element_ip_addresses",
                column: "ip_address");

            migrationBuilder.CreateIndex(
                name: "ix_network_element_ip_addresses_is_available",
                table: "network_element_ip_addresses",
                column: "is_available");

            migrationBuilder.CreateIndex(
                name: "ix_network_element_ip_addresses_network_element_id",
                table: "network_element_ip_addresses",
                column: "network_element_id");

            migrationBuilder.CreateIndex(
                name: "ix_network_elements_element_type",
                table: "network_elements",
                column: "element_type");

            migrationBuilder.CreateIndex(
                name: "ix_network_elements_hostname",
                table: "network_elements",
                column: "hostname");

            migrationBuilder.CreateIndex(
                name: "ix_network_elements_status",
                table: "network_elements",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_network_elements_tenant_id_hostname",
                table: "network_elements",
                columns: new[] { "tenant_id", "hostname" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_network_interfaces_connected_to_interface_id",
                table: "network_interfaces",
                column: "connected_to_interface_id",
                unique: true,
                filter: "connected_to_interface_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_network_interfaces_network_element_id",
                table: "network_interfaces",
                column: "network_element_id");

            migrationBuilder.CreateIndex(
                name: "ix_olts_hostname",
                table: "olts",
                column: "hostname");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_created_at",
                table: "outbox_messages",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_at",
                table: "outbox_messages",
                column: "processed_at");

            migrationBuilder.CreateIndex(
                name: "ix_pon_ports_olt_id_port_number",
                table: "pon_ports",
                columns: new[] { "olt_id", "port_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subnets_network",
                table: "subnets",
                column: "network");

            migrationBuilder.CreateIndex(
                name: "ix_subnets_tenant_id_network",
                table: "subnets",
                columns: new[] { "tenant_id", "network" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vlans_tenant_id_vlan_id",
                table: "vlans",
                columns: new[] { "tenant_id", "vlan_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vlans_vlan_id",
                table: "vlans",
                column: "vlan_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "capacity_records");

            migrationBuilder.DropTable(
                name: "connectivity_links");

            migrationBuilder.DropTable(
                name: "fiber_cables");

            migrationBuilder.DropTable(
                name: "network_element_ip_addresses");

            migrationBuilder.DropTable(
                name: "network_interfaces");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "pon_ports");

            migrationBuilder.DropTable(
                name: "subnets");

            migrationBuilder.DropTable(
                name: "topology_maps");

            migrationBuilder.DropTable(
                name: "vlans");

            migrationBuilder.DropTable(
                name: "network_elements");

            migrationBuilder.DropTable(
                name: "olts");
        }
    }
}
