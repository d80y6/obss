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
$DOCKER_COMPOSE up -d --wait 2>&1 | while IFS= read -r line; do
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

# ── Step 4: Run EF Core Migrations ──────────────────────────────────────────
print_header "Running EF Core Migrations"

log_info "Applying IAM module migrations..."
dotnet ef database update \
    -p "$ROOT_DIR/src/Modules/IAM/Obss.IAM.Infrastructure" \
    -s "$ROOT_DIR/src/Host/Obss.Host" \
    -c IamDbContext \
    --connection "Host=$POSTGRES_HOST;Port=$POSTGRES_PORT;Database=$POSTGRES_DB;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD" \
    2>&1 | while IFS= read -r line; do
    log_info "  ef: $line"
done
log_ok "IAM migrations applied."

log_info "Applying CRM module migrations..."
dotnet ef database update \
    -p "$ROOT_DIR/src/Modules/CRM/Obss.CRM.Infrastructure" \
    -s "$ROOT_DIR/src/Host/Obss.Host" \
    -c CrmDbContext \
    --connection "Host=$POSTGRES_HOST;Port=$POSTGRES_PORT;Database=$POSTGRES_DB;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD" \
    2>&1 | while IFS= read -r line; do
    log_info "  ef: $line"
done
log_ok "CRM migrations applied."

log_info "Applying ProductCatalog module migrations..."
dotnet ef database update \
    -p "$ROOT_DIR/src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure" \
    -s "$ROOT_DIR/src/Host/Obss.Host" \
    -c CatalogDbContext \
    --connection "Host=$POSTGRES_HOST;Port=$POSTGRES_PORT;Database=$POSTGRES_DB;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD" \
    2>&1 | while IFS= read -r line; do
    log_info "  ef: $line"
done
log_ok "ProductCatalog migrations applied."

log_info "Applying Billing module migrations..."
dotnet ef database update \
    -p "$ROOT_DIR/src/Modules/Billing/Obss.Billing.Infrastructure" \
    -s "$ROOT_DIR/src/Host/Obss.Host" \
    -c BillingDbContext \
    --connection "Host=$POSTGRES_HOST;Port=$POSTGRES_PORT;Database=$POSTGRES_DB;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD" \
    2>&1 | while IFS= read -r line; do
    log_info "  ef: $line"
done
log_ok "Billing migrations applied."

log_info "Applying Collections module migrations..."
dotnet ef database update \
    -p "$ROOT_DIR/src/Modules/Collections/Obss.Collections.Infrastructure" \
    -s "$ROOT_DIR/src/Host/Obss.Host" \
    -c CollectionsDbContext \
    --connection "Host=$POSTGRES_HOST;Port=$POSTGRES_PORT;Database=$POSTGRES_DB;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD" \
    2>&1 | while IFS= read -r line; do
    log_info "  ef: $line"
done
log_ok "Collections migrations applied."

# ── Step 5: Apply Seed Data ──────────────────────────────────────────────────
print_header "Applying Seed Data"

SEED_SCRIPT="$ROOT_DIR/infrastructure/database/seed/seed-data.sql"
if [ -f "$SEED_SCRIPT" ]; then
    log_info "Loading seed data into PostgreSQL..."
    docker exec -i obss-postgres \
        psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" < "$SEED_SCRIPT"
    log_ok "Seed data applied successfully."
else
    log_warn "Seed script not found at $SEED_SCRIPT. Skipping."
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
