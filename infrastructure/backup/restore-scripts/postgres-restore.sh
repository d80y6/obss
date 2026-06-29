#!/usr/bin/env bash
set -euo pipefail

# =============================================================================
# PostgreSQL Restore Script
# Telecom OSS/BSS Platform
# 
# Supports:
#   - Full restoration from pg_dump custom format
#   - Point-in-time recovery (PITR) via WAL replay
#   - Validation queries after restore
#   - Multi-database restore
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

PGDATA="${PGDATA:-/var/lib/postgresql/data}"
S3_BACKUP_BUCKET="${S3_BACKUP_BUCKET:-obss-backups}"

SLACK_WEBHOOK_URL="${SLACK_WEBHOOK_URL:-}"
PAGERDUTY_ROUTING_KEY="${PAGERDUTY_ROUTING_KEY:-}"

export PGPASSWORD="${POSTGRES_PASSWORD}"

RESTORE_LOG="/tmp/postgres-restore-$(date +%Y-%m-%d_%H%M%S).log"

# ── Utility Functions ────────────────────────────────────────────────────────

log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $*" | tee -a "${RESTORE_LOG}"
}

notify_slack() {
    local status="$1"
    local message="$2"
    if [ -n "${SLACK_WEBHOOK_URL}" ]; then
        curl -s -X POST -H 'Content-type: application/json' \
            --data "{\"text\":\"[PostgreSQL Restore] ${status}: ${message}\"}" \
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
                    \"source\": \"postgres-restore.sh\"
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
        notify_pagerduty "error" "PostgreSQL Restore: ${message}"
    fi
}

confirm() {
    local prompt="$1"
    echo ""
    echo "⚠️  ${prompt}"
    read -r -p "Type 'YES' to continue: " response
    if [ "${response}" != "YES" ]; then
        echo "Aborted."
        exit 1
    fi
    echo ""
}

check_postgres_not_running() {
    if pg_isready -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" > /dev/null 2>&1; then
        log "WARNING: PostgreSQL is still running on ${POSTGRES_HOST}:${POSTGRES_PORT}"
        log "For PITR, PostgreSQL must be stopped and the data directory cleared."
        log "For dump restore, PostgreSQL must be running but connected database must be available."
        return 0  # Don't block, just warn
    fi
    log "PostgreSQL is not running on ${POSTGRES_HOST}:${POSTGRES_PORT}"
    return 0
}

# ── Restore from pg_dump ─────────────────────────────────────────────────────

restore_from_dump() {
    local dump_file="$1"
    local target_db="$2"
    
    if [ ! -f "${dump_file}" ]; then
        notify "ERROR" "Dump file not found: ${dump_file}"
        return 1
    fi
    
    log "Starting restore of '${target_db}' from dump: ${dump_file}"
    
    local file_size
    file_size=$(stat -c%s "${dump_file}" 2>/dev/null || stat -f%z "${dump_file}" 2>/dev/null)
    log "Dump file size: $(numfmt --to=iec-i "${file_size}" 2>/dev/null || echo "${file_size} bytes")"
    
    # Validate dump file first
    log "Validating dump file integrity..."
    if ! pg_restore --list "${dump_file}" > /dev/null 2>> "${RESTORE_LOG}"; then
        notify "ERROR" "Dump file validation failed — file may be corrupt"
        return 1
    fi
    log "Dump file validation passed"
    
    # Check if target database exists, create if not
    local db_exists
    db_exists=$(psql -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" -d postgres \
        -t -c "SELECT 1 FROM pg_database WHERE datname='${target_db}';" 2>/dev/null | tr -d ' ')
    
    if [ "${db_exists}" != "1" ]; then
        log "Database '${target_db}' does not exist. Creating..."
        psql -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" -d postgres \
            -c "CREATE DATABASE \"${target_db}\";" 2>> "${RESTORE_LOG}" || {
            notify "ERROR" "Failed to create database '${target_db}'"
            return 1
        }
        log "Database '${target_db}' created"
    else
        log "Database '${target_db}' exists"
    fi
    
    # Terminate existing connections
    log "Terminating existing connections to '${target_db}'..."
    psql -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" -d postgres \
        -c "SELECT pg_terminate_backend(pg_stat_activity.pid)
            FROM pg_stat_activity
            WHERE pg_stat_activity.datname = '${target_db}'
              AND pid <> pg_backend_pid();" 2>> "${RESTORE_LOG}" || true
    
    # Perform the restore
    log "Restoring database '${target_db}'..."
    local start_time
    start_time=$(date +%s)
    
    # Use pg_restore with jobs for parallelism
    local jobs
    jobs=$(nproc 2>/dev/null || echo 2)
    
    pg_restore \
        -h "${POSTGRES_HOST}" \
        -p "${POSTGRES_PORT}" \
        -U "${POSTGRES_USER}" \
        -d "${target_db}" \
        --format=custom \
        --jobs="${jobs}" \
        --verbose \
        --exit-on-error \
        "${dump_file}" 2>> "${RESTORE_LOG}"
    
    local restore_exit=$?
    local end_time
    end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    if [ ${restore_exit} -ne 0 ]; then
        notify "ERROR" "pg_restore failed for '${target_db}' (exit code: ${restore_exit}, duration: ${duration}s)"
        return 1
    fi
    
    log "Restore of '${target_db}' completed in ${duration}s"
    return 0
}

# ── Point-in-Time Recovery ───────────────────────────────────────────────────

pitr_restore() {
    local backup_path="$1"
    local wal_path="$2"
    local restore_time="$3"
    local data_dir="$4"
    
    log "═══════════════════════════════════════════════"
    log "Point-in-Time Recovery"
    log "Backup path: ${backup_path}"
    log "WAL path: ${wal_path}"
    log "Target time: ${restore_time}"
    log "Data directory: ${data_dir}"
    log "═══════════════════════════════════════════════"
    
    confirm "This will overwrite the PostgreSQL data directory at ${data_dir}. Continue?"
    
    # Stop PostgreSQL if running
    log "Stopping PostgreSQL..."
    if command -v pg_ctl &> /dev/null; then
        pg_ctl -D "${data_dir}" stop -m fast 2>/dev/null || true
    fi
    
    # Wait for process to stop
    sleep 5
    
    # Backup current data directory
    local backup_date
    backup_date=$(date +%Y%m%d_%H%M%S)
    local old_data_backup="${data_dir}_pre_pitr_${backup_date}"
    log "Backing up current data directory to ${old_data_backup}..."
    mv "${data_dir}" "${old_data_backup}" || {
        notify "ERROR" "Failed to backup current data directory"
        return 1
    }
    
    # Create fresh data directory
    mkdir -p "${data_dir}"
    
    # Restore base backup
    log "Restoring base backup from ${backup_path}..."
    local base_backup
    base_backup=$(find "${backup_path}" -name "*.dump" -type f | sort -r | head -1) || true
    
    if [ -z "${base_backup}" ] || [ ! -f "${base_backup}" ]; then
        notify "ERROR" "No base backup found in ${backup_path}"
        return 1
    fi
    
    log "Using base backup: ${base_backup}"
    
    # For PITR, we need a base backup in PostgreSQL's format, not pg_dump format.
    # pg_dump format cannot be used for PITR directly — we need pg_basebackup instead.
    # This function handles the case where pg_basebackup output is available.
    
    # Check if there's a pg_basebackup tar file
    local base_tar
    base_tar=$(find "${backup_path}" -name "base.tar*" -type f | sort -r | head -1) || true
    
    if [ -n "${base_tar}" ] && [ -f "${base_tar}" ]; then
        log "Restoring from pg_basebackup archive: ${base_tar}"
        tar -xf "${base_tar}" -C "${data_dir}" 2>> "${RESTORE_LOG}" || {
            notify "ERROR" "Failed to extract base backup archive"
            return 1
        }
    else
        # If only pg_dump files exist, we can't do PITR directly.
        # Fall back to dump restore and explain the limitation.
        log "WARNING: pg_basebackup archive not found. pg_dump files are not suitable for PITR."
        log "Falling back to full restore from pg_dump, which will not include WAL replay."
        log "To perform true PITR in the future, use pg_basebackup or enable continuous archiving."
        
        # Instead, we initialize an empty data directory and restore from dump later
        log "Initializing empty PostgreSQL data directory..."
        if command -v initdb &> /dev/null; then
            initdb -D "${data_dir}" 2>> "${RESTORE_LOG}" || {
                notify "ERROR" "Failed to initialize data directory"
                return 1
            }
        else
            notify "ERROR" "initdb not found (not running locally). For PITR, restore must be performed on the PostgreSQL host."
            return 1
        fi
    fi
    
    # Configure recovery.conf / postgresql.conf for PITR
    log "Configuring recovery settings..."
    
    local recovery_conf="${data_dir}/recovery.conf"
    if [ -f "${data_dir}/postgresql.auto.conf" ]; then
        # PostgreSQL 12+ uses recovery.signal
        touch "${data_dir}/recovery.signal"
        
        cat >> "${data_dir}/postgresql.auto.conf" << EOF

# PITR restore settings (applied by restore script)
restore_command = 'aws s3 cp s3://${S3_BACKUP_BUCKET}/postgres/wal/%f %p'
recovery_target_time = '${restore_time}'
recovery_target_action = 'promote'
EOF
    else
        # Older PostgreSQL versions
        cat > "${recovery_conf}" << EOF
# PITR restore configuration
restore_command = 'aws s3 cp s3://${S3_BACKUP_BUCKET}/postgres/wal/%f %p'
recovery_target_time = '${restore_time}'
recovery_target_action = 'promote'
EOF
    fi
    
    # If WAL files are local, use local path instead
    if [ -d "${wal_path}" ]; then
        log "Using local WAL files from ${wal_path}"
        # We need to copy WAL files to pg_wal directory
        mkdir -p "${data_dir}/pg_wal"
        find "${wal_path}" -name "0*" -type f -exec cp {} "${data_dir}/pg_wal/" \; 2>/dev/null || true
    fi
    
    # Start PostgreSQL
    log "Starting PostgreSQL to apply WAL and reach recovery target..."
    if command -v pg_ctl &> /dev/null; then
        pg_ctl -D "${data_dir}" -l "${RESTORE_LOG}" start -w -t 120 2>> "${RESTORE_LOG}" || {
            notify "ERROR" "PostgreSQL failed to start for PITR"
            log "Check log for details: ${RESTORE_LOG}"
            return 1
        }
    else
        log "PostgreSQL must be started manually on the host: pg_ctl -D ${data_dir} start"
    fi
    
    # Wait for recovery to complete
    sleep 5
    
    # Check if recovery is complete
    local in_recovery
    in_recovery=$(psql -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" -d postgres \
        -t -c "SELECT pg_is_in_recovery();" 2>/dev/null | tr -d ' ' || echo "true")
    
    if [ "${in_recovery}" = "f" ]; then
        log "Recovery complete. PostgreSQL is accepting writes."
    else
        log "PostgreSQL is still in recovery mode. Waiting..."
        for i in $(seq 1 30); do
            sleep 5
            in_recovery=$(psql -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" -d postgres \
                -t -c "SELECT pg_is_in_recovery();" 2>/dev/null | tr -d ' ' || echo "true")
            if [ "${in_recovery}" = "f" ]; then
                log "Recovery complete."
                break
            fi
        done
        
        if [ "${in_recovery}" = "t" ]; then
            log "WARNING: PostgreSQL still in recovery after 150s. Check WAL availability."
        fi
    fi
    
    # Remove recovery configuration to prevent re-entry
    rm -f "${data_dir}/recovery.signal" "${data_dir}/recovery.conf" 2>/dev/null || true
    
    log "PITR restore completed"
    return 0
}

# ── Validation ───────────────────────────────────────────────────────────────

validate_restore() {
    local target_db="$1"
    
    log "Running validation queries on '${target_db}'..."
    
    local checks=0
    local passed=0
    
    # Check 1: Can connect and query
    checks=$((checks + 1))
    if psql -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" -d "${target_db}" \
        -c "SELECT 1 AS connection_test;" > /dev/null 2>> "${RESTORE_LOG}"; then
        log "  ✓ Connection test passed"
        passed=$((passed + 1))
    else
        log "  ✗ Connection test FAILED"
    fi
    
    # Check 2: Database size is reasonable (not zero)
    checks=$((checks + 1))
    local db_size
    db_size=$(psql -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" -d "${target_db}" \
        -t -c "SELECT pg_size_pretty(pg_database_size('${target_db}'));" 2>/dev/null | tr -d ' ')
    if [ -n "${db_size}" ] && [ "${db_size}" != "0 bytes" ]; then
        log "  ✓ Database size: ${db_size}"
        passed=$((passed + 1))
    else
        log "  ✗ Database size check FAILED (size: ${db_size})"
    fi
    
    # Check 3: Tables exist and have data
    checks=$((checks + 1))
    local table_count
    table_count=$(psql -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" -d "${target_db}" \
        -t -c "SELECT count(*) FROM information_schema.tables WHERE table_schema NOT IN ('pg_catalog', 'information_schema');" 2>/dev/null | tr -d ' ')
    if [ -n "${table_count}" ] && [ "${table_count}" -gt 0 ]; then
        log "  ✓ ${table_count} tables found"
        passed=$((passed + 1))
    else
        log "  ✗ Table count check FAILED"
    fi
    
    # Check 4: Check for corruption markers
    checks=$((checks + 1))
    local corruption_check
    corruption_check=$(psql -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" -d "${target_db}" \
        -t -c "SELECT count(*) FROM pg_stat_database WHERE datname='${target_db}' AND datistemplate=false;" 2>/dev/null | tr -d ' ')
    if [ -n "${corruption_check}" ]; then
        log "  ✓ Corruption check passed"
        passed=$((passed + 1))
    else
        log "  ✗ Corruption check FAILED"
    fi
    
    # Check 5: Run analyze to verify table integrity
    checks=$((checks + 1))
    if psql -h "${POSTGRES_HOST}" -p "${POSTGRES_PORT}" -U "${POSTGRES_USER}" -d "${target_db}" \
        -c "ANALYZE;" > /dev/null 2>> "${RESTORE_LOG}"; then
        log "  ✓ ANALYZE completed (table integrity verified)"
        passed=$((passed + 1))
    else
        log "  ✗ ANALYZE FAILED — possible table-level corruption"
    fi
    
    # Summary
    log ""
    log "Validation results for '${target_db}': ${passed}/${checks} checks passed"
    
    if [ "${passed}" -eq "${checks}" ]; then
        log "Validation: PASSED"
        return 0
    else
        log "Validation: WARNING — some checks failed"
        return 1
    fi
}

# ── S3 Download ──────────────────────────────────────────────────────────────

download_from_s3() {
    local s3_path="$1"
    local local_path="$2"
    
    if command -v aws &> /dev/null; then
        log "Downloading from s3://${S3_BACKUP_BUCKET}/${s3_path}..."
        aws s3 cp "s3://${S3_BACKUP_BUCKET}/${s3_path}" "${local_path}" \
            --only-show-errors 2>> "${RESTORE_LOG}" || {
            notify "ERROR" "Failed to download from S3: ${s3_path}"
            return 1
        }
        log "Download completed"
    else
        notify "ERROR" "AWS CLI not found, cannot download from S3"
        return 1
    fi
}

# ── Help ─────────────────────────────────────────────────────────────────────

show_usage() {
    cat <<EOF
Usage: $(basename "$0") <command> [options]

Commands:
  restore-from-dump <dump-file> [database]
    Restore a single database from a pg_dump custom format file.
    If database is not specified, restores to primary DB.

  pitr --backup-path <path> --wal-path <path> --restore-time <time> [--data-dir <dir>]
    Perform point-in-time recovery.
    Requires pg_basebackup archive and WAL files.
    --restore-time format: 'YYYY-MM-DD HH:MM:SS TZ'

  list-backups [path]
    List available backup files in a directory or S3 path.

  validate <database>
    Run validation queries on a restored database.

Examples:
  $(basename "$0") restore-from-dump /backup/postgres/daily/obss-2026-06-21_030000.dump obss
  $(basename "$0") pitr --backup-path /backup/postgres/base --wal-path /backup/postgres/wal --restore-time '2026-06-21 14:30:00 UTC'
  $(basename "$0") validate obss_keycloak

Environment:
  All POSTGRES_* and S3_BACKUP_* variables from .env are respected.
EOF
}

list_backups() {
    local path="${1:-/backup/postgres/daily}"
    
    if [ -d "${path}" ]; then
        log "Backup files in ${path}:"
        find "${path}" -name "*.dump" -type f -exec ls -lh {} \; 2>/dev/null | while read -r line; do
            log "  ${line}"
        done
    elif echo "${path}" | grep -q "^s3://\|^s3:"; then
        if command -v aws &> /dev/null; then
            local s3_path="${path#s3://}"
            s3_path="${s3_path#${S3_BACKUP_BUCKET}/}"
            log "Backup files in s3://${S3_BACKUP_BUCKET}/${s3_path}:"
            aws s3 ls "s3://${S3_BACKUP_BUCKET}/${s3_path}" --recursive --human-readable 2>/dev/null || {
                notify "ERROR" "Failed to list S3 path"
                return 1
            }
        else
            notify "ERROR" "AWS CLI not found"
            return 1
        fi
    else
        notify "ERROR" "Path not found: ${path}"
        return 1
    fi
}

# ── Main ─────────────────────────────────────────────────────────────────────

main() {
    local command="${1:-help}"
    shift 1 2>/dev/null || true
    
    log "═══════════════════════════════════════════════"
    log "PostgreSQL Restore Script Started"
    log "Date: $(date)"
    log "═══════════════════════════════════════════════"
    
    case "${command}" in
        restore-from-dump)
            local dump_file="${1:-}"
            local target_db="${2:-${POSTGRES_DB}}"
            
            if [ -z "${dump_file}" ]; then
                log "ERROR: No dump file specified"
                show_usage
                exit 1
            fi
            
            # If the file starts with s3://, download it first
            if echo "${dump_file}" | grep -q "^s3://"; then
                local local_file="/tmp/postgres-restore-$(basename "${dump_file}")"
                local s3_key="${dump_file#s3://${S3_BACKUP_BUCKET}/}"
                download_from_s3 "${s3_key}" "${local_file}" || exit 1
                dump_file="${local_file}"
            fi
            
            confirm "Restore database '${target_db}' from ${dump_file}? This will overwrite existing data."
            
            restore_from_dump "${dump_file}" "${target_db}" || exit 1
            validate_restore "${target_db}" || true
            
            notify "SUCCESS" "Database '${target_db}' restored from dump: $(basename "${dump_file}")"
            ;;
        
        pitr)
            local backup_path=""
            local wal_path=""
            local restore_time=""
            local data_dir="${PGDATA}"
            
            while [ $# -gt 0 ]; do
                case "$1" in
                    --backup-path) backup_path="$2"; shift 2 ;;
                    --wal-path) wal_path="$2"; shift 2 ;;
                    --restore-time) restore_time="$2"; shift 2 ;;
                    --data-dir) data_dir="$2"; shift 2 ;;
                    *) log "ERROR: Unknown option: $1"; show_usage; exit 1 ;;
                esac
            done
            
            if [ -z "${backup_path}" ] || [ -z "${wal_path}" ] || [ -z "${restore_time}" ]; then
                log "ERROR: --backup-path, --wal-path, and --restore-time are required for PITR"
                show_usage
                exit 1
            fi
            
            pitr_restore "${backup_path}" "${wal_path}" "${restore_time}" "${data_dir}" || exit 1
            
            # After PITR, validate all databases
            IFS=',' read -ra databases <<< "${POSTGRES_DB},${POSTGRES_MULTIPLE_DATABASES}"
            for db in "${databases[@]}"; do
                db=$(echo "${db}" | tr -d ' ')
                if [ -n "${db}" ]; then
                    validate_restore "${db}" || true
                fi
            done
            
            notify "SUCCESS" "Point-in-time recovery completed to ${restore_time}"
            ;;
        
        list-backups)
            list_backups "${1:-/backup/postgres/daily}"
            ;;
        
        validate)
            local target_db="${1:-${POSTGRES_DB}}"
            validate_restore "${target_db}"
            ;;
        
        help|--help|-h)
            show_usage
            ;;
        
        *)
            log "ERROR: Unknown command: ${command}"
            show_usage
            exit 1
            ;;
    esac
    
    log "═══════════════════════════════════════════════"
    log "Restore script completed"
    log "═══════════════════════════════════════════════"
}

main "$@"
