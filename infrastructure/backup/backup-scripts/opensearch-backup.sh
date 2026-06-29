#!/usr/bin/env bash
set -euo pipefail

# =============================================================================
# OpenSearch Backup Script
# Telecom OSS/BSS Platform
# 
# Performs:
#   - Snapshot repository registration (S3)
#   - Snapshot creation via API
#   - Snapshot retention management
#   - Health check before/after backup
# =============================================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../../.." && pwd)"

# Load environment
if [ -f "${PROJECT_ROOT}/.env" ]; then
    set -a; source "${PROJECT_ROOT}/.env"; set +a
fi

# ── Configuration ────────────────────────────────────────────────────────────

OPENSEARCH_HOST="${OPENSEARCH_HOST:-localhost}"
OPENSEARCH_PORT="${OPENSEARCH_PORT:-9200}"
OPENSEARCH_ADMIN_USER="${OPENSEARCH_ADMIN_USER:-admin}"
OPENSEARCH_ADMIN_PASSWORD="${OPENSEARCH_ADMIN_PASSWORD:-}"

OPENSEARCH_BASE_URL="http://${OPENSEARCH_HOST}:${OPENSEARCH_PORT}"

SNAPSHOT_REPOSITORY="${SNAPSHOT_REPOSITORY:-obss-backups}"
S3_BACKUP_BUCKET="${S3_BACKUP_BUCKET:-obss-backups}"
S3_BACKUP_BASE_PATH="${S3_BACKUP_BASE_PATH:-opensearch/snapshots}"
S3_REGION="${S3_REGION:-us-east-1}"

SNAPSHOT_PREFIX="${SNAPSHOT_PREFIX:-obss-snapshot}"
MAX_SNAPSHOTS="${MAX_SNAPSHOTS:-14}"  # 14 days of 6-hourly snapshots

SLACK_WEBHOOK_URL="${SLACK_WEBHOOK_URL:-}"
PAGERDUTY_ROUTING_KEY="${PAGERDUTY_ROUTING_KEY:-}"

SNAPSHOT_NAME="${SNAPSHOT_PREFIX}-$(date +%Y-%m-%d-%H%M%S)"

LOGFILE="${BACKUP_DIR:-/backup/opensearch}/logs/opensearch-backup-$(date +%Y-%m-%d_%H%M%S).log"

# Ensure directories
mkdir -p "$(dirname "${LOGFILE}")"

# ── Utility Functions ────────────────────────────────────────────────────────

log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $*" | tee -a "${LOGFILE}"
}

notify_slack() {
    local status="$1"
    local message="$2"
    if [ -n "${SLACK_WEBHOOK_URL}" ]; then
        curl -s -X POST -H 'Content-type: application/json' \
            --data "{\"text\":\"[OpenSearch Backup] ${status}: ${message}\"}" \
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
                    \"source\": \"opensearch-backup.sh\"
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
        notify_pagerduty "error" "OpenSearch Backup: ${message}"
    fi
}

cleanup() {
    local exit_code=$?
    if [ $exit_code -ne 0 ]; then
        notify "ERROR" "Backup script failed with exit code ${exit_code}"
    fi
}

trap cleanup EXIT

opensearch_api() {
    local method="$1"
    local endpoint="$2"
    local data="${3:-}"
    
    local curl_cmd=(curl -s -S -u "${OPENSEARCH_ADMIN_USER}:${OPENSEARCH_ADMIN_PASSWORD}" -X "${method}")
    curl_cmd+=(-H "Content-Type: application/json")
    
    if [ -n "${data}" ]; then
        curl_cmd+=(-d "${data}")
    fi
    
    # OpenSearch may return HTTP 200 with error body for some failures
    local response
    response=$("${curl_cmd[@]}" "${OPENSEARCH_BASE_URL}${endpoint}" 2>&1) || {
        echo "CURL_ERROR: ${response}"
        return 1
    }
    
    echo "${response}"
}

# ── Health Check ─────────────────────────────────────────────────────────────

health_check() {
    log "Running OpenSearch health check..."
    
    local health
    health=$(opensearch_api "GET" "/_cluster/health") || {
        notify "ERROR" "OpenSearch cluster health check failed"
        return 1
    }
    
    local status
    status=$(echo "${health}" | python3 -c "import sys, json; print(json.load(sys.stdin).get('status', 'unknown'))" 2>/dev/null || \
             echo "${health}" | jq -r '.status // "unknown"' 2>/dev/null)
    
    local node_count
    node_count=$(echo "${health}" | python3 -c "import sys, json; print(json.load(sys.stdin).get('number_of_nodes', 0))" 2>/dev/null || \
                 echo "${health}" | jq -r '.number_of_nodes // 0' 2>/dev/null)
    
    log "Cluster status: ${status}, nodes: ${node_count}"
    
    if [ "${status}" = "red" ]; then
        notify "ERROR" "OpenSearch cluster status is RED — backup may be incomplete"
        return 1
    fi
    
    log "OpenSearch health check passed"
    return 0
}

# ── Snapshot Repository ──────────────────────────────────────────────────────

register_repository() {
    log "Registering snapshot repository: ${SNAPSHOT_REPOSITORY}"
    
    # Check if repository already exists
    local repo_check
    repo_check=$(opensearch_api "GET" "/_snapshot/${SNAPSHOT_REPOSITORY}") || true
    
    if echo "${repo_check}" | python3 -c "import sys, json; d = json.load(sys.stdin); sys.exit(0 if '${SNAPSHOT_REPOSITORY}' in d else 1)" 2>/dev/null; then
        log "Repository '${SNAPSHOT_REPOSITORY}' already exists"
        return 0
    fi
    
    # Register S3 repository
    local repo_config
    repo_config=$(cat <<EOF
{
    "type": "s3",
    "settings": {
        "bucket": "${S3_BACKUP_BUCKET}",
        "base_path": "${S3_BACKUP_BASE_PATH}",
        "region": "${S3_REGION}",
        "compress": true,
        "server_side_encryption": true
    }
}
EOF
)
    
    local response
    response=$(opensearch_api "PUT" "/_snapshot/${SNAPSHOT_REPOSITORY}" "${repo_config}") || {
        notify "ERROR" "Failed to register snapshot repository: ${response}"
        return 1
    }
    
    if echo "${response}" | python3 -c "import sys, json; d = json.load(sys.stdin); sys.exit(0 if d.get('acknowledged') else 1)" 2>/dev/null; then
        log "Snapshot repository registered successfully"
    else
        # If S3 plugin not available, try using shared filesystem
        log "WARNING: S3 repository registration may have issue, attempting alternative..."
        local fs_repo_config
        fs_repo_config=$(cat <<EOF
{
    "type": "fs",
    "settings": {
        "location": "/usr/share/opensearch/backup",
        "compress": true
    }
}
EOF
)
        local fs_response
        fs_response=$(opensearch_api "PUT" "/_snapshot/${SNAPSHOT_REPOSITORY}" "${fs_repo_config}") || {
            notify "ERROR" "Failed to register any snapshot repository"
            return 1
        }
        log "Registered filesystem-based snapshot repository instead of S3"
    fi
    
    return 0
}

# ── Create Snapshot ──────────────────────────────────────────────────────────

create_snapshot() {
    log "Creating snapshot: ${SNAPSHOT_NAME}"
    
    # Include all indices, don't wait for completion (monitor in loop)
    local snapshot_config
    snapshot_config=$(cat <<EOF
{
    "indices": "*",
    "ignore_unavailable": true,
    "include_global_state": true,
    "partial": false
}
EOF
)
    
    local response
    response=$(opensearch_api "PUT" "/_snapshot/${SNAPSHOT_REPOSITORY}/${SNAPSHOT_NAME}?wait_for_completion=false" "${snapshot_config}") || {
        notify "ERROR" "Failed to start snapshot: ${response}"
        return 1
    }
    
    if echo "${response}" | python3 -c "import sys, json; d = json.load(sys.stdin); sys.exit(0 if d.get('accepted') else 1)" 2>/dev/null; then
        log "Snapshot started, monitoring progress..."
    else
        local error_reason
        error_reason=$(echo "${response}" | python3 -c "import sys, json; d = json.load(sys.stdin); print(d.get('error', {}).get('reason', 'unknown'))" 2>/dev/null || echo "unknown")
        notify "ERROR" "Snapshot not accepted: ${error_reason}"
        return 1
    fi
    
    # Monitor snapshot progress
    local max_retries=60
    local retry=0
    local snapshot_status="IN_PROGRESS"
    
    while [ "${snapshot_status}" = "IN_PROGRESS" ] || [ "${snapshot_status}" = "STARTED" ]; do
        sleep 10
        
        local status_response
        status_response=$(opensearch_api "GET" "/_snapshot/${SNAPSHOT_REPOSITORY}/${SNAPSHOT_NAME}/_status") || {
            retry=$((retry + 1))
            if [ "${retry}" -ge 5 ]; then
                notify "ERROR" "Failed to query snapshot status"
                return 1
            fi
            continue
        }
        
        retry=0
        
        # Extract status safely
        snapshot_status=$(echo "${status_response}" | python3 -c "
import sys, json
try:
    data = json.load(sys.stdin)
    snapshots = data.get('snapshots', [])
    if snapshots:
        print(snapshots[0].get('state', 'UNKNOWN'))
    else:
        print('UNKNOWN')
except:
    print('UNKNOWN')
" 2>/dev/null || echo "UNKNOWN")
        
        # Get shard stats for progress
        local total_shards
        local done_shards
        total_shards=$(echo "${status_response}" | python3 -c "
import sys, json
try:
    data = json.load(sys.stdin)
    s = data['snapshots'][0]
    print(s['shards_stats']['total'])
except: print(0)
" 2>/dev/null || echo "0")
        done_shards=$(echo "${status_response}" | python3 -c "
import sys, json
try:
    data = json.load(sys.stdin)
    s = data['snapshots'][0]
    print(s['shards_stats']['done'])
except: print(0)
" 2>/dev/null || echo "0")
        
        log "  Snapshot status: ${snapshot_status} (${done_shards}/${total_shards} shards)"
        
        max_retries=$((max_retries - 1))
        if [ "${max_retries}" -le 0 ]; then
            notify "ERROR" "Snapshot timed out after 10 minutes"
            return 1
        fi
    done
    
    if [ "${snapshot_status}" = "SUCCESS" ]; then
        # Get final details
        local final_details
        final_details=$(opensearch_api "GET" "/_snapshot/${SNAPSHOT_REPOSITORY}/${SNAPSHOT_NAME}") || true
        
        local start_time
        local duration
        start_time=$(echo "${final_details}" | python3 -c "
import sys, json
try:
    d = json.load(sys.stdin)
    snapshots = d.get('snapshots', [d])
    s = snapshots[0]
    import datetime
    print(s.get('start_time', 'unknown'))
except: print('unknown')
" 2>/dev/null)
        duration=$(echo "${final_details}" | python3 -c "
import sys, json
try:
    d = json.load(sys.stdin)
    snapshots = d.get('snapshots', [d])
    s = snapshots[0]
    if 'start_time_in_millis' in s and 'end_time_in_millis' in s:
        elapsed = (s['end_time_in_millis'] - s['start_time_in_millis']) / 1000
        print(f'{elapsed:.1f}s')
    else:
        print('unknown')
except: print('unknown')
" 2>/dev/null)
        
        log "Snapshot '${SNAPSHOT_NAME}' completed successfully (duration: ${duration}, started: ${start_time})"
        return 0
    else
        local failure_reason
        failure_reason=$(echo "${status_response}" | python3 -c "
import sys, json
try:
    data = json.load(sys.stdin)
    snapshots = data.get('snapshots', [])
    if snapshots:
        print(snapshots[0].get('failure', 'unknown'))
    else:
        print('unknown')
except: print('unknown')
" 2>/dev/null || echo "unknown")
        
        notify "ERROR" "Snapshot failed with status '${snapshot_status}': ${failure_reason}"
        return 1
    fi
}

# ── Snapshot Restore ─────────────────────────────────────────────────────────

restore_snapshot() {
    local snapshot_name="$1"
    
    log "Restoring snapshot: ${snapshot_name}"
    
    # Check snapshot exists
    local snapshot_check
    snapshot_check=$(opensearch_api "GET" "/_snapshot/${SNAPSHOT_REPOSITORY}/${snapshot_name}") || {
        notify "ERROR" "Snapshot '${snapshot_name}' not found"
        return 1
    }
    
    local snapshot_state
    snapshot_state=$(echo "${snapshot_check}" | python3 -c "
import sys, json
try:
    d = json.load(sys.stdin)
    snapshots = d.get('snapshots', [d])
    print(snapshots[0].get('state', 'UNKNOWN'))
except: print('UNKNOWN')
" 2>/dev/null || echo "UNKNOWN")
    
    if [ "${snapshot_state}" != "SUCCESS" ]; then
        notify "ERROR" "Cannot restore snapshot '${snapshot_name}': state is ${snapshot_state}"
        return 1
    fi
    
    # Close all indices before restore
    log "Closing all indices before restore..."
    local close_response
    close_response=$(opensearch_api "POST" "/_all/_close") || {
        log "WARNING: Failed to close some indices"
    }
    
    # Restore snapshot
    local restore_config='{"indices": "*", "ignore_unavailable": true, "include_global_state": true}'
    local restore_response
    restore_response=$(opensearch_api "POST" "/_snapshot/${SNAPSHOT_REPOSITORY}/${snapshot_name}/_restore?wait_for_completion=true" "${restore_config}") || {
        notify "ERROR" "Snapshot restore failed: ${restore_response}"
        return 1
    }
    
    if echo "${restore_response}" | python3 -c "import sys, json; d = json.load(sys.stdin); sys.exit(0 if d.get('accepted') else 1)" 2>/dev/null; then
        log "Restore initiated. Waiting for completion..."
        # Wait for cluster recovery
        sleep 15
        local recovery_status
        recovery_status=$(opensearch_api "GET" "/_cluster/health?wait_for_status=yellow&timeout=120s") || true
        log "Cluster health after restore: $(echo "${recovery_status}" | python3 -c "import sys, json; print(json.load(sys.stdin).get('status', 'unknown'))" 2>/dev/null || echo "unknown")"
        log "Snapshot restore completed"
    else
        local error_reason
        error_reason=$(echo "${restore_response}" | python3 -c "import sys, json; d = json.load(sys.stdin); print(d.get('error', {}).get('reason', 'unknown'))" 2>/dev/null || echo "unknown")
        notify "ERROR" "Restore failed: ${error_reason}"
        return 1
    fi
}

# ── Retention Management ─────────────────────────────────────────────────────

enforce_retention() {
    log "Enforcing snapshot retention (keeping last ${MAX_SNAPSHOTS})..."
    
    local snapshots_response
    snapshots_response=$(opensearch_api "GET" "/_snapshot/${SNAPSHOT_REPOSITORY}/_all") || {
        log "WARNING: Failed to list snapshots for retention"
        return 1
    }
    
    local snapshot_list
    snapshot_list=$(echo "${snapshots_response}" | python3 -c "
import sys, json
try:
    data = json.load(sys.stdin)
    snapshots = data.get('snapshots', [])
    # Sort by start time (oldest first)
    snapshots.sort(key=lambda s: s.get('start_time_in_millis', 0))
    for s in snapshots:
        print(f\"{s['snapshot']}|{s.get('state', 'UNKNOWN')}|{s.get('start_time', '')}\")
except Exception as e:
    print(f'ERROR: {e}')
" 2>/dev/null || echo "ERROR")
    
    if echo "${snapshot_list}" | grep -q "^ERROR"; then
        log "WARNING: Could not parse snapshot list for retention"
        return 1
    fi
    
    # Filter for successful snapshots with our prefix
    local our_snapshots=()
    while IFS='|' read -r name state start_time; do
        if [[ "${name}" == ${SNAPSHOT_PREFIX}* ]] && [ "${state}" = "SUCCESS" ]; then
            our_snapshots+=("${name}")
        fi
    done <<< "${snapshot_list}"
    
    local count=${#our_snapshots[@]}
    if [ "${count}" -le "${MAX_SNAPSHOTS}" ]; then
        log "Snapshot count (${count}) within retention limit (${MAX_SNAPSHOTS})"
        return 0
    fi
    
    # Delete oldest snapshots beyond retention
    local to_delete=$((count - MAX_SNAPSHOTS))
    for ((i=0; i<to_delete; i++)); do
        local snapshot_to_delete="${our_snapshots[$i]}"
        log "Deleting old snapshot: ${snapshot_to_delete}"
        
        local delete_response
        delete_response=$(opensearch_api "DELETE" "/_snapshot/${SNAPSHOT_REPOSITORY}/${snapshot_to_delete}") || {
            log "WARNING: Failed to delete snapshot ${snapshot_to_delete}"
            continue
        }
        
        if echo "${delete_response}" | python3 -c "import sys, json; d = json.load(sys.stdin); sys.exit(0 if d.get('acknowledged') else 1)" 2>/dev/null; then
            log "Deleted snapshot: ${snapshot_to_delete}"
        else
            log "WARNING: Could not confirm deletion of ${snapshot_to_delete}"
        fi
    done
    
    log "Retention enforcement completed: removed ${to_delete} old snapshot(s)"
}

# ── List Snapshots ──────────────────────────────────────────────────────────

list_snapshots() {
    log "Listing snapshots in repository '${SNAPSHOT_REPOSITORY}'..."
    
    local response
    response=$(opensearch_api "GET" "/_snapshot/${SNAPSHOT_REPOSITORY}/_all") || {
        notify "ERROR" "Failed to list snapshots"
        return 1
    }
    
    echo "${response}" | python3 -c "
import sys, json
try:
    data = json.load(sys.stdin)
    snapshots = data.get('snapshots', [])
    if not snapshots:
        print('No snapshots found')
    else:
        snapshots.sort(key=lambda s: s.get('start_time_in_millis', 0), reverse=True)
        print(f\"{'Snapshot':<40} {'State':<12} {'Start Time':<30} {'Duration':<10}\")
        print('-' * 92)
        for s in snapshots:
            dur = ''
            if 'start_time_in_millis' in s and 'end_time_in_millis' in s:
                secs = (s['end_time_in_millis'] - s['start_time_in_millis']) / 1000
                dur = f'{secs:.1f}s'
            print(f\"{s.get('snapshot', '?'):<40} {s.get('state', '?'):<12} {s.get('start_time', '?'):<30} {dur:<10}\")
except Exception as e:
    print(f'Error: {e}')
" 2>/dev/null || echo "${response}" | python3 -m json.tool 2>/dev/null || echo "${response}"
}

# ── Main ─────────────────────────────────────────────────────────────────────

main() {
    local action="${1:-backup}"
    
    case "${action}" in
        backup)
            log "═══════════════════════════════════════════════"
            log "OpenSearch Backup Script Started"
            log "Host: ${OPENSEARCH_HOST}:${OPENSEARCH_PORT}"
            log "Date: $(date)"
            log "═══════════════════════════════════════════════"
            
            health_check || exit 1
            register_repository || exit 1
            create_snapshot || exit 1
            enforce_retention
            
            log "═══════════════════════════════════════════════"
            log "Backup Summary:"
            log "  Snapshot: ${SNAPSHOT_NAME}"
            log "  Repository: ${SNAPSHOT_REPOSITORY}"
            log "═══════════════════════════════════════════════"
            
            notify "SUCCESS" "OpenSearch snapshot '${SNAPSHOT_NAME}' completed"
            ;;
        
        restore)
            local snapshot_name="${2:-}"
            if [ -z "${snapshot_name}" ]; then
                echo "Usage: $0 restore <snapshot-name>"
                echo ""
                echo "To list available snapshots, run: $0 list"
                exit 1
            fi
            restore_snapshot "${snapshot_name}"
            ;;
        
        list)
            list_snapshots
            ;;
        
        health)
            health_check
            ;;
        
        *)
            echo "Usage: $0 {backup|restore <snapshot>|list|health}"
            echo ""
            echo "Commands:"
            echo "  backup                  Create a new snapshot"
            echo "  restore <snapshot>      Restore from a snapshot"
            echo "  list                    List all snapshots"
            echo "  health                  Check cluster health"
            exit 1
            ;;
    esac
}

main "$@"
