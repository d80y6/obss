#!/usr/bin/env bash
set -euo pipefail

# =============================================================================
# OBSS Platform - Development Environment Setup Script
# =============================================================================
# This script provisions a complete local development environment including:
#   - Dependency checks (dotnet, docker, node)
#   - Docker infrastructure startup
#   - PostgreSQL readiness wait
#   - EF Core migrations
#   - Seed data loading
#   - Solution build validation
# =============================================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
INFRA_DIR="$ROOT_DIR/infrastructure"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

log_info()  { echo -e "${CYAN}[INFO]${NC}  $*"; }
log_ok()    { echo -e "${GREEN}[OK]${NC}    $*"; }
log_warn()  { echo -e "${YELLOW}[WARN]${NC}  $*"; }
log_error() { echo -e "${RED}[ERROR]${NC} $*"; }

cleanup() {
    local exit_code=$?
    if [ $exit_code -ne 0 ]; then
        log_error "Setup failed with exit code $exit_code"
        log_warn "Check the logs above for details."
        log_warn "Run 'docker-compose down' to clean up if needed."
    fi
    exit $exit_code
}
trap cleanup EXIT

# ── Color output for headers ─────────────────────────────────────────────────
print_header() {
    echo ""
    echo "========================================"
    echo "  $1"
    echo "========================================"
}

# ── Step 1: Check Prerequisites ─────────────────────────────────────────────
print_header "Checking Prerequisites"

check_command() {
    local cmd=$1
    local name=$2
    local min_version=$3

    if ! command -v "$cmd" &>/dev/null; then
        log_error "$name is not installed. Please install $name (minimum version: $min_version)."
        exit 1
    fi

    log_ok "$name found: $($cmd --version 2>&1 | head -n 1)"
}

check_command "dotnet"  ".NET SDK"       "9.0"
check_command "docker"  "Docker"         "24.0"
check_command "node"    "Node.js"        "18.0"

# Check Docker Compose (plugin or standalone)
if docker compose version &>/dev/null; then
    DOCKER_COMPOSE="docker compose"
    log_ok "Docker Compose (plugin): $(docker compose version)"
elif docker-compose --version &>/dev/null; then
    DOCKER_COMPOSE="docker-compose"
    log_ok "Docker Compose (standalone): $(docker-compose --version)"
else
    log_error "Docker Compose is not installed."
    exit 1
fi

log_ok "All prerequisites satisfied."

# ── Step 2: Start Docker Infrastructure ──────────────────────────────────────
print_header "Starting Docker Infrastructure"

log_info "Starting Docker services..."
if [ -f "$ROOT_DIR/.env" ]; then
    log_info "Loading environment from .env file"
    export "$(grep -v '^\s*#' "$ROOT_DIR/.env" | grep -v '^\s*$' | xargs)"
fi

cd "$ROOT_DIR" || exit 1
HOST_IP=$(hostname -I | awk '{print $1}') $DOCKER_COMPOSE up -d --wait 2>&1 | while IFS= read -r line; do
    log_info "  docker: $line"
done

log_ok "Docker services are running."

# ── Step 3: Wait for PostgreSQL ──────────────────────────────────────────────
print_header "Waiting for PostgreSQL"

POSTGRES_HOST="${POSTGRES_HOST:-postgres}"
POSTGRES_PORT="${POSTGRES_PORT:-5432}"
POSTGRES_USER="${POSTGRES_USER:-obss_admin}"
POSTGRES_DB="${POSTGRES_DB:-obss}"
POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-obss_s3cur3_p@ss}"
RETRIES=30
RETRY_DELAY=2

log_info "Waiting for PostgreSQL at $POSTGRES_HOST:$POSTGRES_PORT..."

for i in $(seq 1 $RETRIES); do
    if docker exec obss-postgres pg_isready -U "$POSTGRES_USER" -d "$POSTGRES_DB" &>/dev/null; then
        log_ok "PostgreSQL is ready (attempt $i)"
        break
    fi
    if [ "$i" -eq "$RETRIES" ]; then
        log_error "PostgreSQL did not become ready after $RETRIES attempts."
        exit 1
    fi
    log_info "  Waiting for PostgreSQL... ($i/$RETRIES)"
    sleep "$RETRY_DELAY"
done

# ── Step 4: Run EF Core Migrations (all 25 modules) ─────────────────────────
print_header "Running EF Core Migrations"

# Each module uses its own database (obss_{db_name}) matching the runtime DbConn pattern
MIGRATIONS=(
  "IAM/Obss.IAM.Infrastructure:IamDbContext:iam"
  "CRM/Obss.CRM.Infrastructure:CrmDbContext:crm"
  "ProductCatalog/Obss.ProductCatalog.Infrastructure:CatalogDbContext:catalog"
  "Orders/Obss.Orders.Infrastructure:OrderDbContext:orders"
  "Subscriptions/Obss.Subscriptions.Infrastructure:SubscriptionDbContext:subscriptions"
  "Rating/Obss.Rating.Infrastructure:RatingDbContext:rating"
  "Billing/Obss.Billing.Infrastructure:BillingDbContext:billing"
  "Invoices/Obss.Invoices.Infrastructure:InvoiceDbContext:invoices"
  "Payments/Obss.Payments.Infrastructure:PaymentDbContext:payments"
  "Collections/Obss.Collections.Infrastructure:CollectionDbContext:collections"
  "ServiceInventory/Obss.ServiceInventory.Infrastructure:ServiceDbContext:service_inventory"
  "NetworkInventory/Obss.NetworkInventory.Infrastructure:NetworkDbContext:network_inventory"
  "NumberInventory/Obss.NumberInventory.Infrastructure:NumberDbContext:number_inventory"
  "Provisioning/Obss.Provisioning.Infrastructure:ProvisioningDbContext:provisioning"
  "Workflow/Obss.Workflow.Infrastructure:WorkflowDbContext:workflow"
  "Ticketing/Obss.Ticketing.Infrastructure:TicketDbContext:ticketing"
  "Notifications/Obss.Notifications.Infrastructure:NotificationDbContext:notifications"
  "Reporting/Obss.Reporting.Infrastructure:ReportDbContext:reporting"
  "Audit/Obss.Audit.Infrastructure:AuditDbContext:audit"
  "ApiGateway/Obss.ApiGateway.Infrastructure:GatewayDbContext:gateway"
  "ServiceCatalog/Obss.ServiceCatalog.Infrastructure:ServiceCatalogDbContext:service_catalog"
  "EventManagement/Obss.EventManagement.Infrastructure:EventDbContext:event_management"
  "ServiceQualification/Obss.ServiceQualification.Infrastructure:ServiceQualificationDbContext:service_qualification"
  "AAA/Obss.AAA.Infrastructure:AaaDbContext:aaa"
  "OCS/Obss.OCS.Infrastructure:OcsDbContext:ocs"
)

for entry in "${MIGRATIONS[@]}"; do
  IFS=':' read -r proj ctx db <<< "$entry"
  log_info "Applying ${proj%%/*} module migrations (obss_${db})..."
  dotnet ef database update \
    -p "$ROOT_DIR/src/Modules/${proj}" \
    -s "$ROOT_DIR/src/Host/Obss.Host" \
    -c "${ctx}" \
    --connection "Host=$POSTGRES_HOST;Port=$POSTGRES_PORT;Database=obss_${db};Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD" \
    2>&1 | while IFS= read -r line; do
    log_info "  ef: $line"
  done
  log_ok "${proj%%/*} migrations applied."
done

# ── Step 5: Apply Seed Data ──────────────────────────────────────────────────
print_header "Applying Seed Data"

SEEDER="$ROOT_DIR/scripts/seed-databases.sh"
if [ -f "$SEEDER" ]; then
    log_info "Seeding per-module databases..."
    bash "$SEEDER"
    log_ok "All databases seeded."
else
    log_warn "Seeder script not found at $SEEDER. Skipping."
fi

# ── Step 6: Build Solution ───────────────────────────────────────────────────
print_header "Building Solution"

log_info "Building the .NET solution..."
dotnet build "$ROOT_DIR/Obss.sln" \
    --configuration Development \
    --no-restore \
    2>&1 | while IFS= read -r line; do
    log_info "  build: $line"
done
log_ok "Solution build completed."

# ── Step 7: Print Success Message ────────────────────────────────────────────
print_header "Setup Complete"

echo ""
echo -e "${GREEN}  OBSS Platform development environment is ready!${NC}"
echo ""
echo "  Service URLs:"
echo "    PostgreSQL:     localhost:5432 (db: $POSTGRES_DB)"
echo "    Redis:          redis-cli -h localhost -p 6379"
echo "    RabbitMQ:       http://localhost:15672 (user: ${RABBITMQ_USER:-obss_admin})"
echo "    MinIO Console:  http://localhost:9001 (user: ${MINIO_ROOT_USER:-obss_admin})"
echo "    OpenSearch:     http://localhost:9200"
echo "    Keycloak:       http://localhost:8080 (user: ${KEYCLOAK_ADMIN_USER:-admin})"
echo "    Prometheus:     http://localhost:9090"
echo "    Grafana:        http://localhost:3000 (user: ${GRAFANA_ADMIN_USER:-admin})"
echo "    API Gateway:    http://localhost:5000"
echo ""
echo "  Quick commands:"
echo "    docker-compose ps              View running services"
echo "    docker-compose logs -f <svc>   Tail service logs"
echo "    docker-compose down            Stop all services"
echo "    ./scripts/reset-db.sh          Reset database & re-seed"
echo ""

if [ -f "$ROOT_DIR/.env" ]; then
    log_warn "Default passwords are set in .env. Change them for production!"
fi
