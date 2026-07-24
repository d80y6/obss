#!/usr/bin/env bash
# ==============================================================================
# OBSS Platform - Database Seeder
# Routes seed data to the correct per-module databases.
# ==============================================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SEED_DIR="${SCRIPT_DIR}/../infrastructure/database/seed"
CONTAINER_NAME="${DOCKER_CONTAINER:-obss-postgres}"
PG_USER="${POSTGRES_USER:-obss_admin}"

if docker ps --format '{{.Names}}' 2>/dev/null | grep -q "^${CONTAINER_NAME}$"; then
    PSQL="docker exec -i ${CONTAINER_NAME} psql -U ${PG_USER}"
else
    echo "ERROR: PostgreSQL container '${CONTAINER_NAME}' not running"
    exit 1
fi

run_seed() {
    local db="$1" file="$2" label="${3:-}"
    label="${label:-$(basename "$file")}"
    echo "[seed]  Seeding ${db} (${label})..."
    $PSQL -d "${db}" < "${file}" 2>&1 | awk '!/^(BEGIN|COMMIT|ROLLBACK|psql:.*NOTICE|INSERT 0|CREATE|TABLE|ALTER)/ {if (NR > 2) print "  " $0}'
    echo "[seed]  ✓ ${db} seeded"
}

echo "===== OBSS Database Seeder ====="
echo ""

# ── Phase 1: Core IAM (tenants, roles, users, permissions) ─────────────────
run_seed "obss_iam" "${SEED_DIR}/seed-iam.sql"

# ── Phase 2: CRM (customer segments) ───────────────────────────────────────
run_seed "obss_crm" "${SEED_DIR}/seed-crm.sql"

# ── Phase 3: Catalog (categories) ──────────────────────────────────────────
run_seed "obss_catalog" "${SEED_DIR}/seed-catalog.sql"

# ── Phase 4: Billing reference data (currencies) ───────────────────────────
run_seed "obss_billing" "${SEED_DIR}/seed-billing.sql"

# ── Phase 4b: AAA (NAS devices) ──────────────────────────────────────────────
run_seed "obss_aaa" "${SEED_DIR}/seed-aaa.sql"

echo ""
echo "--- Base seed data complete ---"
echo ""

# ── Phase 5: Yemen PTC Catalog (per-database extracts) ─────────────────────
# Yemen PTC IAM tenant
run_seed "obss_iam" "${SEED_DIR}/yemen-ptc-iam-part.sql"

# Yemen PTC CRM segments
run_seed "obss_crm" "${SEED_DIR}/yemen-ptc-crm-part.sql"

# Yemen PTC Catalog
run_seed "obss_catalog" "${SEED_DIR}/yemen-ptc-catalog-part.sql"

# Yemen PTC Service Catalog
run_seed "obss_service_catalog" "${SEED_DIR}/yemen-ptc-service-catalog-part.sql"

echo ""
echo "===== ALL DATABASES SEEDED SUCCESSFULLY ====="
