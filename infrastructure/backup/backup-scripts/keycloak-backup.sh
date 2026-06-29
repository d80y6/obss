#!/usr/bin/env bash
set -euo pipefail

# =============================================================================
# Keycloak Backup Script
# Telecom OSS/BSS Platform
# 
# Performs:
#   - Realm export via Keycloak Admin REST API
#   - User federation configuration export
#   - Backup encryption with GPG
#   - Upload to S3-compatible storage
# =============================================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../../.." && pwd)"

# Load environment
if [ -f "${PROJECT_ROOT}/.env" ]; then
    set -a; source "${PROJECT_ROOT}/.env"; set +a
fi

# ── Configuration ────────────────────────────────────────────────────────────

KEYCLOAK_HOST="${KEYCLOAK_HOST:-localhost}"
KEYCLOAK_PORT="${KEYCLOAK_PORT:-8080}"
KEYCLOAK_ADMIN="${KEYCLOAK_ADMIN_USER:-admin}"
KEYCLOAK_ADMIN_PASSWORD="${KEYCLOAK_ADMIN_PASSWORD:-}"
KEYCLOAK_REALM="${KEYCLOAK_REALM:-obss}"

KEYCLOAK_BASE_URL="http://${KEYCLOAK_HOST}:${KEYCLOAK_PORT}"
KEYCLOAK_REALM_URL="${KEYCLOAK_BASE_URL}/admin/realms"

BACKUP_DIR="${BACKUP_DIR:-/backup/keycloak}"
S3_BACKUP_BUCKET="${S3_BACKUP_BUCKET:-obss-backups}"
S3_BACKUP_PREFIX="${S3_BACKUP_PREFIX:-keycloak}"

GPG_PASSPHRASE="${GPG_PASSPHRASE:-}"
GPG_RECIPIENT="${GPG_RECIPIENT:-}"

SLACK_WEBHOOK_URL="${SLACK_WEBHOOK_URL:-}"
PAGERDUTY_ROUTING_KEY="${PAGERDUTY_ROUTING_KEY:-}"

RETENTION_DAYS=30

DATE=$(date +%Y-%m-%d)
DATETIME=$(date +%Y-%m-%d_%H%M%S)

# Ensure directories exist
mkdir -p "${BACKUP_DIR}"/{realms,user-federation,logs}
mkdir -p "${BACKUP_DIR}/restore"

LOGFILE="${BACKUP_DIR}/logs/keycloak-backup-${DATETIME}.log"

# ── Utility Functions ────────────────────────────────────────────────────────

log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $*" | tee -a "${LOGFILE}"
}

notify_slack() {
    local status="$1"
    local message="$2"
    if [ -n "${SLACK_WEBHOOK_URL}" ]; then
        curl -s -X POST -H 'Content-type: application/json' \
            --data "{\"text\":\"[Keycloak Backup] ${status}: ${message}\"}" \
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
                    \"source\": \"keycloak-backup.sh\"
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
        notify_pagerduty "error" "Keycloak Backup: ${message}"
    fi
}

cleanup() {
    local exit_code=$?
    # Clean up temporary files
    rm -f /tmp/keycloak-backup-*.json /tmp/keycloak-backup-*.tar.gz /tmp/keycloak-backup-*.gpg 2>/dev/null || true
    if [ $exit_code -ne 0 ]; then
        notify "ERROR" "Backup script failed with exit code ${exit_code}"
    fi
}

trap cleanup EXIT

get_admin_token() {
    log "Obtaining Keycloak admin token..."
    
    local token_response
    token_response=$(curl -s -S -f \
        -X POST \
        "${KEYCLOAK_BASE_URL}/realms/master/protocol/openid-connect/token" \
        -H "Content-Type: application/x-www-form-urlencoded" \
        -d "client_id=admin-cli" \
        -d "username=${KEYCLOAK_ADMIN}" \
        -d "password=${KEYCLOAK_ADMIN_PASSWORD}" \
        -d "grant_type=password" 2>&1) || {
        notify "ERROR" "Failed to obtain admin token: ${token_response}"
        return 1
    }
    
    echo "${token_response}" | python3 -c "import sys, json; print(json.load(sys.stdin)['access_token'])" 2>/dev/null || {
        # Fallback to jq if python3 not available
        echo "${token_response}" | jq -r '.access_token'
    }
}

keycloak_api() {
    local method="$1"
    local endpoint="$2"
    local data="${3:-}"
    local token="${4:-}"
    
    local curl_cmd=(curl -s -S -X "${method}")
    curl_cmd+=(-H "Authorization: Bearer ${token}")
    curl_cmd+=(-H "Content-Type: application/json")
    curl_cmd+=("${KEYCLOAK_BASE_URL}${endpoint}")
    
    if [ -n "${data}" ]; then
        curl_cmd+=(-d "${data}")
    fi
    
    "${curl_cmd[@]}" 2>&1
}

encrypt_file() {
    local infile="$1"
    local outfile="$2"
    
    log "Encrypting backup file..."
    
    if [ -n "${GPG_RECIPIENT}" ]; then
        # Asymmetric encryption with recipient key
        gpg --batch --yes --trust-model always \
            --recipient "${GPG_RECIPIENT}" \
            --output "${outfile}" \
            --encrypt "${infile}" 2>> "${LOGFILE}" || {
            log "WARNING: GPG asymmetric encryption failed, trying symmetric"
            encrypt_file_symmetric "${infile}" "${outfile}"
        }
    elif [ -n "${GPG_PASSPHRASE}" ]; then
        encrypt_file_symmetric "${infile}" "${outfile}"
    else
        # No encryption configured, copy as-is
        log "WARNING: No encryption configured, backup will be stored unencrypted"
        cp "${infile}" "${outfile}"
    fi
}

encrypt_file_symmetric() {
    local infile="$1"
    local outfile="$2"
    
    echo "${GPG_PASSPHRASE}" | gpg --batch --yes --passphrase-fd 0 \
        --symmetric --cipher-algo AES256 \
        --output "${outfile}" \
        "${infile}" 2>> "${LOGFILE}" || {
        notify "ERROR" "GPG symmetric encryption failed"
        return 1
    }
}

decrypt_file() {
    local infile="$1"
    local outfile="$2"
    
    log "Decrypting backup file..."
    
    if [ -n "${GPG_PASSPHRASE}" ]; then
        echo "${GPG_PASSPHRASE}" | gpg --batch --yes --passphrase-fd 0 \
            --output "${outfile}" \
            --decrypt "${infile}" 2>> "${LOGFILE}" || {
            notify "ERROR" "GPG decryption failed"
            return 1
        }
    elif [ -n "${GPG_RECIPIENT}" ]; then
        gpg --batch --yes \
            --output "${outfile}" \
            --decrypt "${infile}" 2>> "${LOGFILE}" || {
            notify "ERROR" "GPG decryption failed"
            return 1
        }
    else
        cp "${infile}" "${outfile}"
    fi
}

# ── Backup Functions ─────────────────────────────────────────────────────────

export_realms() {
    local token="$1"
    local backup_file="$2"
    
    log "Exporting realm(s) from Keycloak..."
    
    # Get list of realms
    local realms_response
    realms_response=$(keycloak_api "GET" "/admin/realms" "" "${token}") || {
        notify "ERROR" "Failed to list realms"
        return 1
    }
    
    local realm_names
    realm_names=$(echo "${realms_response}" | python3 -c "
import sys, json
realms = json.load(sys.stdin)
for r in realms:
    print(r['realm'])
" 2>/dev/null || echo "${realms_response}" | jq -r '.[].realm')
    
    # Create a temporary directory for realm exports
    local tmpdir
    tmpdir=$(mktemp -d /tmp/keycloak-backup-XXXXXX)
    
    export_realm_full() {
        local realm="$1"
        local output="${tmpdir}/realm-${realm}.json"
        
        log "  Exporting realm: ${realm}"
        
        # Export full realm configuration via Keycloak API
        # Keycloak supports partial export: https://<host>/admin/realms/<realm>/partial-export
        local export_data
        export_data=$(keycloak_api "POST" "/admin/realms/${realm}/partial-export" \
            '{"exportClients":true,"exportGroupsAndRoles":true}' \
            "${token}") || {
            log "  WARNING: Partial export failed for realm ${realm}, trying full export..."
            export_data=$(keycloak_api "GET" "/admin/realms/${realm}" "" "${token}") || {
                log "  WARNING: Could not export realm ${realm}"
                return 1
            }
        }
        
        echo "${export_data}" > "${output}"
        
        # Validate JSON
        echo "${export_data}" | python3 -c "import sys, json; json.load(sys.stdin)" 2>/dev/null || {
            log "  WARNING: Invalid JSON for realm ${realm}, cleaning up..."
            rm -f "${output}"
            return 1
        }
        
        log "  Realm ${realm} exported successfully"
    }
    
    # Export master realm always
    export_realm_full "master" || true
    
    # Export each realm
    for realm in ${realm_names}; do
        if [ "${realm}" != "master" ]; then
            export_realm_full "${realm}" || true
        fi
    done
    
    # Package all realm exports into a tarball
    local tarfile="${tmpdir}/realms-export-${DATETIME}.tar.gz"
    tar -czf "${tarfile}" -C "${tmpdir}" realm-*.json 2>> "${LOGFILE}"
    
    # Copy to backup location
    cp "${tarfile}" "${backup_file}"
    
    # Cleanup
    rm -rf "${tmpdir}"
    
    log "Realm export completed: ${backup_file}"
}

export_user_federation() {
    local token="$1"
    local backup_file="$2"
    
    log "Exporting user federation configurations..."
    
    local tmpdir
    tmpdir=$(mktemp -d /tmp/keycloak-uf-backup-XXXXXX)
    
    # Get list of realms to check for user federation
    local realms_response
    realms_response=$(keycloak_api "GET" "/admin/realms" "" "${token}") || {
        log "WARNING: Failed to list realms for user federation export"
        rm -rf "${tmpdir}"
        return 1
    }
    
    local realm_names
    realm_names=$(echo "${realms_response}" | python3 -c "
import sys, json
realms = json.load(sys.stdin)
for r in realms:
    print(r['realm'])
" 2>/dev/null || echo "${realms_response}" | jq -r '.[].realm')
    
    for realm in ${realm_names}; do
        # Export LDAP/User Federation providers
        local uf_response
        uf_response=$(keycloak_api "GET" "/admin/realms/${realm}/components?type=org.keycloak.storage.UserStorageProvider" "" "${token}") || {
            log "  WARNING: No user federation providers found for realm ${realm}"
            continue
        }
        
        local uf_count
        uf_count=$(echo "${uf_response}" | python3 -c "
import sys, json
providers = json.load(sys.stdin)
print(len(providers))
" 2>/dev/null || echo "0")
        
        if [ "${uf_count}" -gt 0 ]; then
            echo "${uf_response}" > "${tmpdir}/uf-${realm}.json"
            log "  Exported ${uf_count} user federation provider(s) for realm ${realm}"
        fi
    done
    
    # Package user federation exports
    local tarfile="${tmpdir}/user-federation-${DATETIME}.tar.gz"
    tar -czf "${tarfile}" -C "${tmpdir}" uf-*.json 2>/dev/null || {
        log "No user federation configurations found"
        rm -rf "${tmpdir}"
        touch "${backup_file}"  # Create empty file to track the backup
        return 0
    }
    
    cp "${tarfile}" "${backup_file}"
    rm -rf "${tmpdir}"
    
    log "User federation export completed"
}

export_client_secrets() {
    local token="$1"
    local backup_file="$2"
    
    log "Exporting client secrets (encrypted)..."
    
    local tmpdir
    tmpdir=$(mktemp -d /tmp/keycloak-cs-backup-XXXXXX)
    local secrets_file="${tmpdir}/client-secrets.json"
    
    echo "{" > "${secrets_file}"
    local first=true
    
    local realms_response
    realms_response=$(keycloak_api "GET" "/admin/realms" "" "${token}") || {
        rm -rf "${tmpdir}"
        return 0
    }
    
    local realm_names
    realm_names=$(echo "${realms_response}" | python3 -c "
import sys, json
realms = json.load(sys.stdin)
for r in realms:
    print(r['realm'])
" 2>/dev/null || echo "${realms_response}" | jq -r '.[].realm')
    
    for realm in ${realm_names}; do
        local clients_response
        clients_response=$(keycloak_api "GET" "/admin/realms/${realm}/clients" "" "${token}") || continue
        
        local client_info
        client_info=$(echo "${clients_response}" | python3 -c "
import sys, json
clients = json.load(sys.stdin)
result = {}
for c in clients:
    if c.get('secret'):
        result[c['clientId']] = {
            'id': c['id'],
            'secret': c['secret'],
            'serviceAccountsEnabled': c.get('serviceAccountsEnabled', False)
        }
print(json.dumps(result, indent=2))
" 2>/dev/null || echo "{}")
        
        if [ "${client_info}" != "{}" ]; then
            if [ "${first}" = true ]; then
                first=false
            else
                echo "," >> "${secrets_file}"
            fi
            echo "\"${realm}\": ${client_info}" >> "${secrets_file}"
        fi
    done
    
    echo "}" >> "${secrets_file}"
    
    # Only create the file if we found secrets
    if [ "$(stat -c%s "${secrets_file}" 2>/dev/null || stat -f%z "${secrets_file}" 2>/dev/null)" -gt 3 ]; then
        cp "${secrets_file}" "${backup_file}"
        log "Client secrets exported"
    else
        touch "${backup_file}"
        log "No client secrets with confidential access type found"
    fi
    
    rm -rf "${tmpdir}"
}

# ── Restore Function ─────────────────────────────────────────────────────────

perform_restore() {
    local backup_file="$1"
    
    if [ ! -f "${backup_file}" ]; then
        notify "ERROR" "Backup file not found: ${backup_file}"
        exit 1
    fi
    
    log "Starting Keycloak restore from: ${backup_file}"
    
    # Check if file is encrypted (check GPG magic bytes)
    local is_encrypted=false
    if file "${backup_file}" | grep -q "GPG"; then
        is_encrypted=true
    fi
    
    local tmpdir
    tmpdir=$(mktemp -d /tmp/keycloak-restore-XXXXXX)
    local decrypted_file="${tmpdir}/restore.tar.gz"
    
    if [ "${is_encrypted}" = true ]; then
        decrypt_file "${backup_file}" "${decrypted_file}" || {
            rm -rf "${tmpdir}"
            exit 1
        }
    else
        cp "${backup_file}" "${decrypted_file}"
    fi
    
    # Extract
    tar -xzf "${decrypted_file}" -C "${tmpdir}" 2>> "${LOGFILE}" || {
        notify "ERROR" "Failed to extract backup archive"
        rm -rf "${tmpdir}"
        exit 1
    }
    
    # Get admin token
    local token
    token=$(get_admin_token) || {
        rm -rf "${tmpdir}"
        exit 1
    }
    
    # Restore each realm
    for realm_file in "${tmpdir}"/realm-*.json; do
        if [ -f "${realm_file}" ]; then
            local realm_name
            realm_name=$(basename "${realm_file}" .json | sed 's/^realm-//')
            
            log "Restoring realm: ${realm_name}"
            
            # Check if realm exists
            local check_response
            check_response=$(keycloak_api "GET" "/admin/realms/${realm_name}" "" "${token}") || true
            
            if echo "${check_response}" | python3 -c "import sys, json; d = json.load(sys.stdin); sys.exit(0 if 'realm' in d else 1)" 2>/dev/null; then
                log "  Realm ${realm_name} already exists, skipping"
            else
                # Create realm
                local realm_data
                realm_data=$(python3 -c "
import sys, json
with open('${realm_file}') as f:
    data = json.load(f)
# Strip fields that can't be set on creation
for field in ['id', 'users', 'roles', 'groups', 'clients', 'scopeMappings']:
    data.pop(field, None)
print(json.dumps(data))
")
                
                local create_response
                create_response=$(keycloak_api "POST" "/admin/realms" "${realm_data}" "${token}") || {
                    log "  WARNING: Failed to create realm ${realm_name}"
                    continue
                }
                log "  Realm ${realm_name} created"
            fi
        fi
    done
    
    rm -rf "${tmpdir}"
    log "Keycloak restore completed"
}

# ── Retention ────────────────────────────────────────────────────────────────

enforce_retention() {
    log "Enforcing retention policy (keeping last ${RETENTION_DAYS} days)..."
    
    local dir="${BACKUP_DIR}/realms"
    find "${dir}" -name "realms-export-*.tar.gz*" -type f -mtime +${RETENTION_DAYS} 2>/dev/null | while read -r f; do
        rm -f "${f}" "${f}.gpg"
        
        local basename_f
        basename_f=$(basename "${f}")
        if command -v aws &> /dev/null; then
            aws s3 rm "s3://${S3_BACKUP_BUCKET}/${S3_BACKUP_PREFIX}/realms/${basename_f}" --only-show-errors 2>/dev/null || true
            aws s3 rm "s3://${S3_BACKUP_BUCKET}/${S3_BACKUP_PREFIX}/realms/${basename_f}.gpg" --only-show-errors 2>/dev/null || true
        fi
        log "Deleted old backup: ${f}"
    done
    
    # Also clean user federation backups
    find "${BACKUP_DIR}/user-federation" -name "user-federation-*.tar.gz*" -type f -mtime +${RETENTION_DAYS} 2>/dev/null | while read -r f; do
        rm -f "${f}" "${f}.gpg"
        local basename_f
        basename_f=$(basename "${f}")
        if command -v aws &> /dev/null; then
            aws s3 rm "s3://${S3_BACKUP_BUCKET}/${S3_BACKUP_PREFIX}/user-federation/${basename_f}" --only-show-errors 2>/dev/null || true
            aws s3 rm "s3://${S3_BACKUP_BUCKET}/${S3_BACKUP_PREFIX}/user-federation/${basename_f}.gpg" --only-show-errors 2>/dev/null || true
        fi
    done
    
    log "Retention enforcement completed"
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
    else
        log "WARNING: AWS CLI not found, skipping S3 upload"
    fi
}

# ── Main ─────────────────────────────────────────────────────────────────────

main() {
    local action="${1:-backup}"
    
    case "${action}" in
        backup)
            log "═══════════════════════════════════════════════"
            log "Keycloak Backup Script Started"
            log "Host: ${KEYCLOAK_HOST}:${KEYCLOAK_PORT}"
            log "Date: ${DATETIME}"
            log "═══════════════════════════════════════════════"
            
            # Check prerequisites
            if ! command -v gpg &> /dev/null; then
                log "WARNING: GPG not installed, backups will not be encrypted"
            fi
            
            # Get admin token
            local token
            token=$(get_admin_token) || exit 1
            
            # Export realms
            local realms_file="${BACKUP_DIR}/realms/realms-export-${DATETIME}.tar.gz"
            export_realms "${token}" "${realms_file}"
            
            # Export user federation
            local uf_file="${BACKUP_DIR}/user-federation/user-federation-${DATETIME}.tar.gz"
            export_user_federation "${token}" "${uf_file}"
            
            # Export client secrets
            local secrets_file="${BACKUP_DIR}/restore/client-secrets-${DATETIME}.json"
            export_client_secrets "${token}" "${secrets_file}"
            
            # Encrypt backups
            log "Encrypting backup files..."
            for f in "${realms_file}" "${uf_file}"; do
                if [ -f "${f}" ] && [ "$(stat -c%s "${f}" 2>/dev/null || stat -f%z "${f}" 2>/dev/null)" -gt 0 ]; then
                    encrypt_file "${f}" "${f}.gpg"
                    rm -f "${f}"  # Remove unencrypted original
                fi
            done
            
            # Upload to S3
            log "Uploading backups to S3..."
            for f in "${BACKUP_DIR}/realms"/*.gpg; do
                if [ -f "${f}" ]; then
                    upload_to_s3 "${f}" "${S3_BACKUP_PREFIX}/realms/$(basename "${f}")"
                fi
            done
            for f in "${BACKUP_DIR}/user-federation"/*.gpg; do
                if [ -f "${f}" ]; then
                    upload_to_s3 "${f}" "${S3_BACKUP_PREFIX}/user-federation/$(basename "${f}")"
                fi
            done
            
            # Enforce retention
            enforce_retention
            
            # Summary
            log "═══════════════════════════════════════════════"
            log "Backup Summary:"
            ls -lh "${BACKUP_DIR}/realms/"*"${DATETIME}"* 2>/dev/null | while read -r line; do log "  ${line}"; done
            ls -lh "${BACKUP_DIR}/user-federation/"*"${DATETIME}"* 2>/dev/null | while read -r line; do log "  ${line}"; done
            log "═══════════════════════════════════════════════"
            
            notify "SUCCESS" "Keycloak backup completed successfully"
            ;;
        
        restore)
            local backup_file="${2:-}"
            if [ -z "${backup_file}" ]; then
                echo "Usage: $0 restore <backup-file>"
                exit 1
            fi
            perform_restore "${backup_file}"
            ;;
        
        *)
            echo "Usage: $0 {backup|restore <file>}"
            echo ""
            echo "Commands:"
            echo "  backup              Perform full Keycloak backup"
            echo "  restore <file>      Restore Keycloak from backup file"
            exit 1
            ;;
    esac
}

main "$@"
