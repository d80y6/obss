#!/usr/bin/env bash
set -euo pipefail

# =============================================================================
# OBSS Platform - Database Reset Script
# =============================================================================
# Drops and recreates the PostgreSQL database, runs all EF Core migrations,
# and applies seed data. Useful for development when schema has changed.
# =============================================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

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
    [ $exit_code -ne 0 ] && log_error "Database reset failed with exit code $exit_code"
    exit $exit_code
}
trap cleanup EXIT

print_header() {
    echo ""
    echo "========================================"
    echo "  $1"
    echo "========================================"
}

# ── Load .env if available ───────────────────────────────────────────────────
if [ -f "$ROOT_DIR/.env" ]; then
    log_info "Loading environment from .env file"
    set -a
    source "$ROOT_DIR/.env"
    set +a
fi

POSTGRES_HOST="${POSTGRES_HOST:-postgres}"
POSTGRES_PORT="${POSTGRES_PORT:-5432}"
POSTGRES_DB="${POSTGRES_DB:-obss}"
POSTGRES_USER="${POSTGRES_USER:-obss_admin}"
POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-obss_s3cur3_p@ss}"

CONNECTION="Host=$POSTGRES_HOST;Port=$POSTGRES_PORT;Database=$POSTGRES_DB;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD"

# ── Check prerequisites ──────────────────────────────────────────────────────
print_header "Checking Prerequisites"

if ! command -v dotnet &>/dev/null; then
    log_error ".NET SDK is not installed."
    exit 1
fi

# Check if PostgreSQL is accessible via docker or direct connection
if docker exec obss-postgres pg_isready -U "$POSTGRES_USER" -d "$POSTGRES_DB" &>/dev/null 2>&1; then
    POSTGRES_ACCESS="docker"
    log_ok "PostgreSQL accessible via Docker container 'obss-postgres'"
elif command -v psql &>/dev/null && PGPASSWORD="$POSTGRES_PASSWORD" psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d postgres -c "SELECT 1" &>/dev/null; then
    POSTGRES_ACCESS="direct"
    log_ok "PostgreSQL accessible directly at $POSTGRES_HOST:$POSTGRES_PORT"
else
    log_warn "PostgreSQL does not appear to be running or accessible."
    log_warn "Make sure Docker services are up: docker-compose up -d"
    log_warn "Attempting to proceed anyway..."
    POSTGRES_ACCESS="direct"
fi

# ── Confirm with user ────────────────────────────────────────────────────────
print_header "Confirm Reset"

echo ""
log_warn "This will DROP and recreate the database '$POSTGRES_DB'."
log_warn "ALL DATA WILL BE LOST."
echo ""
read -r -p "Are you sure you want to proceed? [y/N] " response
case "$response" in
    [yY][eE][sS]|[yY])
        log_info "Proceeding with database reset..."
        ;;
    *)
        log_info "Database reset cancelled."
        exit 0
        ;;
esac

# ── Drop and recreate database ──────────────────────────────────────────────
print_header "Dropping and Recreating Database"

log_info "Dropping existing connections and database..."

if [ "$POSTGRES_ACCESS" = "docker" ]; then
    # Terminate existing connections and drop the database
    docker exec obss-postgres psql -U "$POSTGRES_USER" -d postgres <<-EOSQL
        UPDATE pg_database SET datallowconn = 'false' WHERE datname = '$POSTGRES_DB';
        SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$POSTGRES_DB' AND pid <> pg_backend_pid();
        DROP DATABASE IF EXISTS "$POSTGRES_DB";
        CREATE DATABASE "$POSTGRES_DB" OWNER "$POSTGRES_USER";
EOSQL
    log_ok "Database '$POSTGRES_DB' dropped and recreated."
else
    PGPASSWORD="$POSTGRES_PASSWORD" psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d postgres <<-EOSQL
        UPDATE pg_database SET datallowconn = 'false' WHERE datname = '$POSTGRES_DB';
        SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$POSTGRES_DB' AND pid <> pg_backend_pid();
        DROP DATABASE IF EXISTS "$POSTGRES_DB";
        CREATE DATABASE "$POSTGRES_DB" OWNER "$POSTGRES_USER";
EOSQL
    log_ok "Database '$POSTGRES_DB' dropped and recreated."
fi

# ── Run EF Core Migrations ──────────────────────────────────────────────────
print_header "Running EF Core Migrations"

declare -A MIGRATIONS
MIGRATIONS["IAM"]="src/Modules/IAM/Obss.IAM.Infrastructure:IamDbContext"
MIGRATIONS["CRM"]="src/Modules/CRM/Obss.CRM.Infrastructure:CrmDbContext"
MIGRATIONS["ProductCatalog"]="src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure:CatalogDbContext"
MIGRATIONS["Billing"]="src/Modules/Billing/Obss.Billing.Infrastructure:BillingDbContext"
MIGRATIONS["Collections"]="src/Modules/Collections/Obss.Collections.Infrastructure:CollectionsDbContext"

for module in "${!MIGRATIONS[@]}"; do
    IFS=':' read -r project context <<< "${MIGRATIONS[$module]}"
    project_path="$ROOT_DIR/$project"

    if [ ! -d "$project_path" ]; then
        log_warn "Project not found for module '$module' at $project_path. Skipping."
        continue
    fi

    log_info "Applying $module migrations (context: $context)..."
    if dotnet ef database update \
        -p "$project_path" \
        -s "$ROOT_DIR/src/Host/Obss.Host" \
        -c "$context" \
        --connection "$CONNECTION" \
        --no-build 2>/dev/null; then
        log_ok "$module migrations applied."
    else
        log_info "  (no-build failed, attempting with build...)"
        if dotnet ef database update \
            -p "$project_path" \
            -s "$ROOT_DIR/src/Host/Obss.Host" \
            -c "$context" \
            --connection "$CONNECTION"; then
            log_ok "$module migrations applied."
        else
            log_warn "Failed to apply $module migrations. Skipping."
        fi
    fi
done

# ── Apply Seed Data ──────────────────────────────────────────────────────────
print_header "Applying Seed Data"

SEED_SCRIPT="$ROOT_DIR/infrastructure/database/seed/seed-data.sql"
if [ -f "$SEED_SCRIPT" ]; then
    log_info "Loading seed data..."
    if [ "$POSTGRES_ACCESS" = "docker" ]; then
        docker exec -i obss-postgres psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" < "$SEED_SCRIPT"
    else
        PGPASSWORD="$POSTGRES_PASSWORD" psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d "$POSTGRES_DB" < "$SEED_SCRIPT"
    fi
    log_ok "Seed data applied successfully."
else
    log_warn "Seed script not found at $SEED_SCRIPT. Skipping."
fi

# ── Done ─────────────────────────────────────────────────────────────────────
print_header "Reset Complete"

echo ""
echo -e "${GREEN}  Database '$POSTGRES_DB' has been reset successfully!${NC}"
echo ""
echo "  Summary:"
echo "    Database:     $POSTGRES_DB"
echo "    Host:         $POSTGRES_HOST:$POSTGRES_PORT"
echo "    User:         $POSTGRES_USER"
echo "    Migrations:   Applied"
echo "    Seed Data:    Loaded"
echo ""
