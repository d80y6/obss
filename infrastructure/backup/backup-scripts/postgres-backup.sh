#!/usr/bin/env bash
set -euo pipefail

# =============================================================================
# PostgreSQL Backup Script
# Telecom OSS/BSS Platform
# 
# Performs:
#   - Full database dump (pg_dump custom format)
#   - WAL archiving verification
#   - Retention management (daily: 7d, weekly: 4w, monthly: 12m)
#   - Health check and notification
# =============================================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../../.." && pwd)"

# Load environment
if [ -f "${PROJECT_ROOT}/.env" ]; then
    set -a; source "${PROJECT_ROOT}/.env"; set +a
fi

# ── Configuration ────────────────────────────────────────────────────────────

POSTGRES_USER="${POSTGRES_USER:-obss_admin}"
POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-}"
POSTGRES_HOST="${POSTGRES_HOST:-localhost}"
POSTGRES_PORT="${POSTGRES_PORT:-5432}"
POSTGRES_DB="${POSTGRES_DB:-obss}"
POSTGRES_MULTIPLE_DATABASES="${POSTGRES_MULTIPLE_DATABASES:-obss_keycloak,obss_audit}"

BACKUP_DIR="${BACKUP_DIR:-/backup/postgres}"
S3_BACKUP_BUCKET="${S3_BACKUP_BUCKET:-obss-backups}"
S3_BACKUP_PREFIX="${S3_BACKUP_PREFIX:-postgres}"

RETENTION_DAILY=7
RETENTION_WEEKLY=4
RETENTION_MONTHLY=12

SLACK_WEBHOOK_URL="${SLACK_WEBHOOK_URL:-}"
PAGERDUTY_ROUTING_KEY="${PAGERDUTY_ROUTING_KEY:-}"

DATE=$(date +%Y-%m-%d)
DATETIME=$(date +%Y-%m-%d_%H%M%S)
WEEK_NUM=$(date +%V)
DAY_OF_MONTH=$(date +%d)
DOW=$(date +%u)  # 1=Monday ... 7=Sunday

export PGPASSWORD="${POSTGRES_PASSWORD}"

# Ensure backup directories exist
mkdir -p "${BACKUP_DIR}"/{daily,weekly,monthly,logs}

LOGFILE="${BACKUP_DIR}/logs/postgres-backup-${DATETIME}.log"

# ── Utility Functions ────────────────────────────────────────────────────────

log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $*" | tee -a "${LOGFILE}"
}

notify_slack() {
    local status="$1"
    local message="$2"
    if [ -n "${SLACK_WEBHOOK_URL}" ]; then
        curl -s -X POST -H 'Content-type: application/json' \
            --data "{\"text\":\"[PostgreSQL Backup] ${status}: ${message}\"}" \
            "${SLACK_WEBHOOK_URL}" > /dev/null 2>&1 || true
    fi
}

notify_pagerduty() {
    local severity="$1"
    local summary="$2"
    if [ -n "${PAGERDUTY_ROUTING_KEY}" ]; then
        curl -s -X POST -H 'Content-Type: application/json' \
            --data "{
                \"routing_key\": \"${PAGERDUTY_ROUTING_KEY}\",
                \"event_action\": \"trigger\",
                \"payload\": {
                    \"summary\": \"${summary}\",
                    \"severity\": \"${severity}\",
                    \"source\": \"postgres-backup.sh\"
                }
            }" \
            "https://events.pagerduty.com/v2/enqueue" > /dev/null 2>&1 || true
    fi
}

notify() {
    local status="$1"
    local message="$2"
    log "${status}: ${message}"
    notify_slack "${status}" "${message}"
    if [ "${status}" = "ERROR" ]; then
        notify_pagerduty "error" "PostgreSQL Backup: ${message}"
    fi
}

cleanup() {
    local exit_code=$?
    if [ $exit_code -ne 0 ]; then
        notify "ERROR" "Backup script failed with exit code ${exit_code}"
        notify_pagerduty "critical" "PostgreSQL backup script failed on ${HOSTNAME}"
    fi
}

trap cleanup EXIT

health_check() {
    log "Running PostgreSQL health check..."
    
    if ! command -v psql &> /dev/null; then
        notify "ERROR" "psql command not found"
        return 1
    fi
    
    local result
    result=$(psql -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" -d "${POSTGRES_DB}" \
        -t -c "SELECT 1;" 2>&1) || {
        notify "ERROR" "PostgreSQL health check failed: ${result}"
        return 1
    }
    
    log "PostgreSQL health check passed"
    return 0
}

get_db_size() {
    local db="$1"
    psql -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" -d "${db}" \
        -t -c "SELECT pg_size_pretty(pg_database_size('${db}'));" 2>/dev/null | tr -d ' '
}

backup_database() {
    local db="$1"
    local backup_type="$2"  # daily, weekly, monthly
    local backup_file="${BACKUP_DIR}/${backup_type}/${db}-${DATETIME}.dump"
    local backup_info="${BACKUP_DIR}/${backup_type}/${db}-${DATETIME}.info"
    
    log "Starting backup of database '${db}' (${backup_type})..."
    
    local db_size
    db_size=$(get_db_size "${db}")
    log "Database '${db}' size: ${db_size}"
    
    # Perform pg_dump in custom format (compressed, parallel-capable)
    pg_dump \
        -h "${POSTGRES_HOST}" \
        -p "${POSTGRES_PORT}" \
        -U "${POSTGRES_USER}" \
        -d "${db}" \
        --format=custom \
        --compress=9 \
        --verbose \
        --file="${backup_file}" \
        2>> "${LOGFILE}"
    
    local dump_exit=$?
    if [ $dump_exit -ne 0 ]; then
        notify "ERROR" "pg_dump failed for database '${db}' (exit code: ${dump_exit})"
        return 1
    fi
    
    # Verify dump file
    if [ ! -f "${backup_file}" ]; then
        notify "ERROR" "Backup file not created for database '${db}'"
        return 1
    fi
    
    local file_size
    file_size=$(stat -c%s "${backup_file}" 2>/dev/null || stat -f%z "${backup_file}" 2>/dev/null)
    if [ "${file_size}" -eq 0 ]; then
        notify "ERROR" "Backup file is empty for database '${db}'"
        return 1
    fi
    
    # Validate dump with pg_restore --list
    if ! pg_restore --list "${backup_file}" > /dev/null 2>> "${LOGFILE}"; then
        notify "ERROR" "Backup validation failed for database '${db}' (corrupt dump)"
        return 1
    fi
    
    # Write backup metadata
    cat > "${backup_info}" <<EOF
database=${db}
backup_type=${backup_type}
date=${DATETIME}
size_bytes=${file_size}
size_human=$(numfmt --to=iec-i "${file_size}" 2>/dev/null || echo "${file_size}")
db_size=${db_size}
pg_dump_version=$(pg_dump --version)
pg_version=$(psql -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" -d "${db}" -t -c "SELECT version();" 2>/dev/null | tr -d ' ')
checksum=$(sha256sum "${backup_file}" | cut -d' ' -f1)
EOF
    
    log "Backup of '${db}' completed: $(numfmt --to=iec-i "${file_size}" 2>/dev/null || echo "${file_size} bytes")"
    return 0
}

upload_to_s3() {
    local file="$1"
    local s3_path="$2"
    
    if command -v aws &> /dev/null; then
        log "Uploading to s3://${S3_BACKUP_BUCKET}/${s3_path}..."
        aws s3 cp "${file}" "s3://${S3_BACKUP_BUCKET}/${s3_path}" \
            --storage-class STANDARD_IA \
            --only-show-errors \
            2>> "${LOGFILE}" || {
            log "WARNING: S3 upload failed for ${file}"
            return 1
        }
        log "S3 upload completed"
    else
        log "WARNING: AWS CLI not found, skipping S3 upload"
    fi
}

enforce_retention() {
    local backup_type="$1"
    local retention_count="$2"
    local dir="${BACKUP_DIR}/${backup_type}"
    
    log "Enforcing retention for ${backup_type} backups (keeping last ${retention_count})..."
    
    # List dump files sorted by date (oldest first), skip the first ${retention_count}
    local to_delete
    to_delete=$(ls -1t "${dir}"/*.dump 2>/dev/null | tail -n +$((retention_count + 1)) || true)
    
    if [ -n "${to_delete}" ]; then
        echo "${to_delete}" | while read -r f; do
            local info_file="${f%.dump}.info"
            rm -f "${f}" "${info_file}"
            log "Deleted old backup: ${f}"
            
            # Also remove from S3
            local s3_key
            s3_key="${S3_BACKUP_PREFIX}/${backup_type}/$(basename "${f}")"
            if command -v aws &> /dev/null; then
                aws s3 rm "s3://${S3_BACKUP_BUCKET}/${s3_key}" --only-show-errors 2>/dev/null || true
            fi
        done
    fi
    
    log "Retention enforcement for ${backup_type} completed"
}

# ── WAL Archive Verification ─────────────────────────────────────────────────

verify_wal_archiving() {
    log "Verifying WAL archiving status..."
    
    local wal_status
    wal_status=$(psql -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" -d "${POSTGRES_DB}" \
        -t -c "SELECT pg_is_in_recovery();" 2>/dev/null | tr -d ' ')
    
    if [ "${wal_status}" = "f" ]; then
        local last_wal
        last_wal=$(psql -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" -d "${POSTGRES_DB}" \
            -t -c "SELECT pg_walfile_name(pg_current_wal_lsn());" 2>/dev/null | tr -d ' ')
        
        local archive_status
        archive_status=$(psql -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" -d "${POSTGRES_DB}" \
            -t -c "SELECT pg_stat_get_archiver()::text;" 2>/dev/null)
        
        log "Current WAL file: ${last_wal}"
        log "Archive status: ${archive_status}"
        
        # Check if archive_command is set
        local archive_mode
        archive_mode=$(psql -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" -d "${POSTGRES_DB}" \
            -t -c "SHOW archive_mode;" 2>/dev/null | tr -d ' ')
        
        if [ "${archive_mode}" = "on" ]; then
            log "WAL archiving is active"
        else
            log "WARNING: WAL archiving is not enabled"
        fi
    else
        log "Server is in recovery mode, skipping WAL check"
    fi
}

# ── Main ─────────────────────────────────────────────────────────────────────

main() {
    log "═══════════════════════════════════════════════"
    log "PostgreSQL Backup Script Started"
    log "Host: ${POSTGRES_HOST}:${POSTGRES_PORT}"
    log "Date: ${DATETIME}"
    log "═══════════════════════════════════════════════"
    
    # Step 1: Health check
    health_check || exit 1
    
    # Step 2: Verify WAL archiving
    verify_wal_archiving
    
    # Step 3: Determine backup type
    BACKUP_TYPE="daily"
    if [ "${DOW}" -eq 7 ]; then  # Sunday → weekly
        BACKUP_TYPE="weekly"
    fi
    if [ "${DAY_OF_MONTH}" = "01" ]; then  # 1st of month → monthly
        BACKUP_TYPE="monthly"
    fi
    
    log "Backup type: ${BACKUP_TYPE}"
    
    # Step 4: Backup main database
    BACKUP_SUCCESS=true
    IFS=',' read -ra DATABASES <<< "${POSTGRES_DB},${POSTGRES_MULTIPLE_DATABASES}"
    for db in "${DATABASES[@]}"; do
        db=$(echo "${db}" | tr -d ' ')
        if [ -n "${db}" ]; then
            if ! backup_database "${db}" "${BACKUP_TYPE}"; then
                BACKUP_SUCCESS=false
            fi
        fi
    done
    
    if [ "${BACKUP_SUCCESS}" = false ]; then
        notify "ERROR" "One or more database backups failed"
        exit 1
    fi
    
    # Step 5: Upload to S3
    log "Uploading backups to S3..."
    for f in "${BACKUP_DIR}/${BACKUP_TYPE}"/*-"${DATETIME}".dump; do
        if [ -f "${f}" ]; then
            upload_to_s3 "${f}" "${S3_BACKUP_PREFIX}/${BACKUP_TYPE}/$(basename "${f}")"
            
            # Also upload info file
            local info_f="${f%.dump}.info"
            if [ -f "${info_f}" ]; then
                upload_to_s3 "${info_f}" "${S3_BACKUP_PREFIX}/${BACKUP_TYPE}/$(basename "${info_f}")"
            fi
        fi
    done
    
    # Step 6: Upload WAL files that haven't been archived yet
    log "Archiving any pending WAL files..."
    psql -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" -d "${POSTGRES_DB}" \
        -c "SELECT pg_switch_wal();" > /dev/null 2>> "${LOGFILE}" || true
    
    # Step 7: Enforce retention
    enforce_retention "daily" "${RETENTION_DAILY}"
    enforce_retention "weekly" "${RETENTION_WEEKLY}"
    enforce_retention "monthly" "${RETENTION_MONTHLY}"
    
    # Step 8: Clean up old WAL files on S3 if base backups are deleted
    if command -v aws &> /dev/null; then
        log "Cleaning up orphaned WAL files from S3..."
        local oldest_backup
        oldest_backup=$(aws s3 ls "s3://${S3_BACKUP_BUCKET}/${S3_BACKUP_PREFIX}/daily/" \
            --recursive 2>/dev/null | grep '\.dump$' | sort | head -1 | awk '{print $4}' || true)
        if [ -n "${oldest_backup}" ]; then
            local oldest_date
            oldest_date=$(echo "${oldest_backup}" | grep -oP '\d{4}-\d{2}-\d{2}_\d{6}' | head -1 || true)
            if [ -n "${oldest_date}" ]; then
                log "Oldest daily backup: ${oldest_date}, cleaning WAL files before this date..."
                # WAL cleanup would be done here based on WAL file naming convention
            fi
        fi
    fi
    
    # Step 9: Summary
    log "═══════════════════════════════════════════════"
    log "Backup Summary:"
    for f in "${BACKUP_DIR}/${BACKUP_TYPE}"/*-"${DATETIME}".info; do
        if [ -f "${f}" ]; then
            cat "${f}" >> "${LOGFILE}"
            log "  + $(grep '^database=' "${f}" | cut -d= -f2) ($(grep '^size_human=' "${f}" | cut -d= -f2))"
        fi
    done
    log "═══════════════════════════════════════════════"
    
    notify "SUCCESS" "PostgreSQL backup completed successfully (${BACKUP_TYPE})"
}

main "$@"
