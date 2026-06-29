# Disaster Recovery Runbook — Telecom OSS/BSS Platform

**Version:** 1.0
**Last Updated:** 2026-06-21
**Owner:** Platform Reliability Team
**Classification:** Internal — Operations Critical

---

## Table of Contents

1. [DR Strategy Overview](#1-dr-strategy-overview)
2. [Backup Strategy](#2-backup-strategy)
3. [Recovery Procedures](#3-recovery-procedures)
4. [DR Testing Schedule](#4-dr-testing-schedule)
5. [Contact Escalation Tree](#5-contact-escalation-tree)
6. [Appendix](#6-appendix)

---

## 1. DR Strategy Overview

### 1.1 Recovery Objectives

| Metric | Target | Notes |
|--------|--------|-------|
| **RTO** (Recovery Time Objective) | 4 hours | Maximum acceptable downtime for full platform recovery |
| **RPO — Transactional** | 1 hour | Maximum acceptable data loss for transactional systems (orders, billing, CRM) |
| **RPO — Reporting** | 24 hours | Maximum acceptable data loss for reporting/analytics systems |

### 1.2 Architecture

The platform runs as a Docker Compose stack on a single host or swarm cluster. DR relies on:

- **Primary site** — Production data center / cloud region
- **Secondary site** — Geo-redundant backup location for snapshots and WAL archives
- **Backup storage** — S3-compatible object storage (MinIO or equivalent) at secondary site

### 1.3 Assumptions

- Secondary site has equivalent infrastructure capacity to run the full stack
- Docker images are available from a registry (not dependent on primary site)
- Network connectivity between sites for replication is available
- DNS can be updated to point to secondary site if needed

### 1.4 Failure Scenarios

| Scenario | RTO | Procedure |
|----------|-----|-----------|
| Single service crash | < 5 min | Docker restart / health check self-heal |
| Host failure (hardware/OS) | 4 hours | Full site failover to secondary |
| Data corruption | Varies | Point-in-time recovery from WAL |
| Ransomware / security incident | 4 hours | Full restore from immutable backups |
| Region outage (cloud) | 4 hours | Full site failover to secondary |
| Accidental deletion | Varies | Restore from last good backup |

---

## 2. Backup Strategy

### 2.1 Backup Matrix

| Component | Method | Frequency | Retention | RPO Achieved |
|-----------|--------|-----------|-----------|--------------|
| PostgreSQL (transactional) | pg_dump + WAL streaming | Daily dump, continuous WAL | Daily: 7d, Weekly: 4w, Monthly: 12m | < 1 hour |
| PostgreSQL (reporting) | pg_dump | Daily | Daily: 7d, Weekly: 4w, Monthly: 12m | < 24 hours |
| Redis | RDB snapshot | Every hour | 24 hours | < 1 hour |
| RabbitMQ | Definitions export | Daily (or after config change) | 30 days | N/A (ephemeral queues rebuilt) |
| MinIO | rclone / s3cmd sync | Continuous (15 min) | 30 days with versioning | < 15 min |
| OpenSearch | Snapshot API | Every 6 hours | 14 days | < 6 hours |
| Keycloak | Realm export | Daily (or after config change) | 30 days | < 24 hours |
| Prometheus | TSDB snapshot | Daily | 30 days | < 24 hours |
| Grafana | Dashboard export | Daily | 30 days | < 24 hours |
| Application configs | Git / tarball | Every deployment | Git history | N/A |

### 2.2 PostgreSQL Backup

**Method:**
- **Logical backup:** `pg_dump --format=custom` run daily via cron (script: `backup-scripts/postgres-backup.sh`)
- **WAL archiving:** Continuous WAL streaming to secondary site via `pg_receivewal` or S3
- **Archive command:** `archive_command = 'aws s3 cp %p s3://backup-bucket/postgres/wal/%f'`

**WAL Configuration (postgresql.conf):**
```ini
wal_level = replica
archive_mode = on
archive_command = 'aws s3 cp %p s3://<backup-bucket>/postgres/wal/%f --storage-class STANDARD_IA'
archive_timeout = 60
max_wal_senders = 5
wal_keep_size = 1024    # MB
```

**Retention:**
- Daily backups: 7 days (local), 7 days (remote)
- Weekly backups: 4 weeks (every Sunday)
- Monthly backups: 12 months (1st of month)
- WAL files: pruned after base backup they belong to is deleted

**Verification:**
- Post-backup: Restore dump to staging DB, run health queries
- Notification: Slack/PagerDuty alert on failure

### 2.3 Redis Backup

**Method:**
- RDB snapshots every hour via `SAVE` or `BGSAVE`
- AOF (Append-Only File) enabled for durability

**Configuration:**
```ini
save 3600 1        # At least 1 change in 3600s → save
appendonly yes
appendfsync everysec
```

**Recovery:**
- Copy RDB file to secondary site
- On restore, place RDB file in Redis data directory and restart

### 2.4 RabbitMQ Backup

**Method:**
- Export definitions via `rabbitmqadmin export rabbitmq.definitions.json`
- Queues are ephemeral; messages are re-published by producers
- Durable queues and bindings restored from definitions export

**Script:**
```bash
rabbitmqadmin -u <user> -p <password> export /backup/rabbitmq/definitions-$(date +%F).json
```

### 2.5 MinIO Backup

**Method:**
- Use `rclone` or `aws s3 sync` to replicate all buckets to secondary S3-compatible storage
- Enable bucket versioning on both source and target

**Script:**
```bash
rclone sync minio-prod: minio-dr: --transfers 8 --checkers 16 --progress
```

### 2.6 OpenSearch Backup

**Method:**
- Register S3 repository for snapshots
- Create snapshots via `_snapshot` API every 6 hours
- Managed by `backup-scripts/opensearch-backup.sh`

### 2.7 Keycloak Backup

**Method:**
- Export realms via Keycloak Admin REST API
- Export user federation configurations
- Encrypt backup with GPG
- Managed by `backup-scripts/keycloak-backup.sh`

### 2.8 Backup Storage Layout

```
backup-bucket/
├── postgres/
│   ├── base/               # pg_dump custom format
│   │   ├── daily/
│   │   ├── weekly/
│   │   └── monthly/
│   └── wal/                # WAL segments
├── keycloak/
│   ├── realms/
│   └── user-federation/
├── opensearch/
│   └── snapshots/
├── redis/
│   └── rdbs/
├── rabbitmq/
│   └── definitions/
├── minio/
│   └── buckets/
├── prometheus/
│   └── snapshots/
└── grafana/
    └── dashboards/
```

### 2.9 Encryption

- All backups stored at rest are encrypted (server-side encryption on S3)
- Keycloak backups use GPG symmetric encryption with a key stored in a secrets manager
- WAL segments are encrypted in transit (TLS) and at rest (S3 SSE)

---

## 3. Recovery Procedures

### 3.1 General Recovery Principles

1. **Assess the scope** — Is it a single service, data corruption, or full site failure?
2. **Stop the bleeding** — Isolate affected systems, prevent cascading failures
3. **Restore from last known good backup** — Follow the appropriate procedure below
4. **Validate integrity** — Run health checks, verification queries
5. **Resume traffic** — Redirect traffic, update DNS if needed
6. **Post-mortem** — Document root cause, improve prevention

### 3.2 Full Site Recovery

Use this procedure when the primary site is completely unavailable (hardware failure, region outage, ransomware).

**Prerequisites:**
- Secondary site infrastructure is provisioned and verified
- Latest backups are available at secondary site
- Docker images are accessible from registry
- DNS records are ready to be updated

**Step-by-Step:**

```
Timeline: 4 hours total
```

| Phase | Time | Action | Owner |
|-------|------|--------|-------|
| **1. Assess** | T+0min | Declare disaster, notify on-call, initiate DR plan | DR Lead |
| **2. Provision** | T+15min | Spin up secondary infrastructure (or verify existing) | Infrastructure |
| **3. Restore PostgreSQL** | T+45min | Restore latest base backup + replay WAL to consistency point | DBA |
| **4. Restore Keycloak** | T+75min | Restore Keycloak realm export, verify authentication | IAM |
| **5. Restore OpenSearch** | T+105min | Restore latest OpenSearch snapshot | Platform |
| **6. Restore Redis** | T+120min | Restore latest Redis RDB | Platform |
| **7. Restore MinIO** | T+135min | Sync MinIO buckets from backup | Platform |
| **8. Restore RabbitMQ** | T+140min | Import definitions, allow queues to populate | Platform |
| **9. Start applications** | T+150min | Deploy Docker Compose stack, run health checks | Platform |
| **10. Validate** | T+180min | Run smoke tests, verify end-to-end flows | QA / Platform |
| **11. Cutover** | T+210min | Update DNS, verify traffic flowing | Network |
| **12. Monitor** | T+240min | Confirm everything operational, declare success | DR Lead |

**Detailed Commands:**

```bash
# Step 3: Restore PostgreSQL
# On the secondary site host:
./restore-scripts/postgres-restore.sh \
  --backup-path s3://backup-bucket/postgres/base/daily/ \
  --wal-path s3://backup-bucket/postgres/wal/ \
  --restore-time "2026-06-21 03:45:00 UTC"

# Step 4: Restore Keycloak
# Start Keycloak with empty data dir, then import realm
./backup-scripts/keycloak-backup.sh --restore --file /backup/keycloak/realms/master-2026-06-21.gpg

# Step 5: Restore OpenSearch
# Register snapshot repo, restore snapshot
./backup-scripts/opensearch-backup.sh --restore --snapshot obss-snapshot-2026-06-21-0600

# Step 6: Restore Redis
cp /backup/redis/rdbs/dump-2026-06-21-0600.rdb /data/redis/dump.rdb
docker compose up -d redis

# Step 7: Restore MinIO
rclone sync minio-dr: minio-prod: --transfers 8

# Step 8: Restore RabbitMQ
rabbitmqadmin import /backup/rabbitmq/definitions-2026-06-21.json

# Step 9: Start applications
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Step 10: Validate
./scripts/health-check.sh
curl -s http://localhost:5000/health | jq .
```

### 3.3 Database Restore Procedure

Use this when PostgreSQL data is corrupted, accidentally deleted, or needs point-in-time recovery.

**Prerequisites:**
- Base backup file (custom format dump)
- WAL archive (for PITR beyond the base backup time)
- Target recovery time

**Step-by-Step:**

1. **Stop the application** to prevent writes:
   ```bash
   docker compose stop obss-api-gateway
   ```

2. **Stop PostgreSQL**:
   ```bash
   docker compose stop postgres
   ```

3. **Restore from dump** (simplest path):
   ```bash
   ./restore-scripts/postgres-restore.sh \
     --dump-file /backup/postgres/base/daily/obss-full-2026-06-21.dump \
     --database obss
   ```

4. **Point-in-time recovery** (for precise recovery point):
   ```bash
   ./restore-scripts/postgres-restore.sh \
     --pitr \
     --backup-path /backup/postgres/base/ \
     --wal-path /backup/postgres/wal/ \
     --restore-time "2026-06-21 14:30:00 UTC" \
     --data-dir /var/lib/postgresql/data
   ```

5. **Validate data integrity**:
   ```bash
   docker compose exec -T postgres psql -U obss_admin -d obss -c "SELECT count(*) FROM information_schema.tables;"
   docker compose exec -T postgres psql -U obss_admin -d obss -c "SELECT count(*) FROM public.\"Orders\";"
   ./scripts/health-check.sh
   ```

6. **Restart application**:
   ```bash
   docker compose up -d obss-api-gateway
   ```

### 3.4 Application Recovery

Use this when a specific service crashes, has a bad deployment, or configuration issue.

**Step-by-Step:**

1. **Check service health**:
   ```bash
   docker compose ps
   docker compose logs --tail=50 obss-api-gateway
   ```

2. **Restart the service**:
   ```bash
   docker compose restart obss-api-gateway
   ```

3. **Roll back to previous version**:
   ```bash
   docker compose stop obss-api-gateway
   docker compose rm obss-api-gateway
   # Revert tag in docker-compose.yml, then:
   docker compose up -d obss-api-gateway
   ```

4. **Scale up if resource-constrained**:
   ```bash
   docker compose up -d --scale obss-api-gateway=3 obss-api-gateway
   ```

### 3.5 Redis Recovery

```bash
# Stop Redis
docker compose stop redis

# Replace dump file
cp /backup/redis/rdbs/dump-2026-06-21-0600.rdb /var/lib/docker/volumes/obss_redis_data/_data/dump.rdb

# Start Redis
docker compose up -d redis

# Verify
docker compose exec redis redis-cli -a <password> ping
```

### 3.6 RabbitMQ Recovery

```bash
# Start a clean RabbitMQ
docker compose up -d rabbitmq

# Wait for it to be healthy, then import definitions
docker compose exec rabbitmq rabbitmqadmin \
  -u obss_admin -p <password> \
  import /path/to/definitions-2026-06-21.json

# Verify queues and bindings
docker compose exec rabbitmq rabbitmqctl list_queues
```

### 3.7 MinIO Recovery

```bash
# Sync from backup
rclone sync minio-dr: minio-prod: --transfers 8 --checkers 16 --progress

# Verify bucket contents
rclone ls minio-prod:
```

---

## 4. DR Testing Schedule

### 4.1 Test Cadence

| Frequency | Test | Scope | Success Criteria |
|-----------|------|-------|------------------|
| **Weekly** | Backup integrity check | All components | Backup files exist, non-zero, recent (< 2 days old) |
| **Monthly** | Single service restore | PostgreSQL, Redis, Keycloak | Service healthy and data verified |
| **Quarterly** | Full DR drill | Full stack failover to DR environment | RTO < 4h, RPO within targets |
| **Annually** | Chaos engineering | Simulate region outage, network partition, data corruption | Platform recovers within RTO/RPO |

### 4.2 DR Drill Checklist (Quarterly)

```
[ ] Pre-drill notification sent to stakeholders (48h in advance)
[ ] DR environment verified: capacity, networking, DNS
[ ] Latest backups confirmed available at DR site
[ ] Backups verified: checksums match, files not corrupted
[ ] All team members briefed on roles
[ ] DR Lead declared drill start: ______ (timestamp)
[ ] Step 1: Infrastructure provisioned: ______
[ ] Step 2: PostgreSQL restored and validated: ______
[ ] Step 3: Keycloak restored and validated: ______
[ ] Step 4: OpenSearch restored and validated: ______
[ ] Step 5: Redis restored and validated: ______
[ ] Step 6: MinIO restored and validated: ______
[ ] Step 7: RabbitMQ restored and validated: ______
[ ] Step 8: Application stack deployed and healthy: ______
[ ] Step 9: Smoke tests passed: ______
[ ] Step 10: DNS cutover tested (if applicable): ______
[ ] Total RTO achieved: ______ hours ______ minutes
[ ] Data loss (RPO) measured: ______
[ ] Post-drill retrospective scheduled
[ ] Action items documented and assigned
[ ] Runbook updated with lessons learned
```

### 4.3 Backup Validation Checklist (Weekly)

```
[ ] PostgreSQL dump files exist and are < 2 days old
[ ] WAL archive directory has recent files (< 1 hour)
[ ] OpenSearch snapshot exists and status is SUCCESS
[ ] Keycloak realm export exists and is < 2 days old
[ ] Redis RDB files exist and are < 2 hours old
[ ] RabbitMQ definitions export exists and is < 2 days old
[ ] MinIO sync log shows successful replication
[ ] Prometheus snapshot exists
[ ] Grafana dashboard export exists
[ ] All backup scripts ran without errors (check /var/log/backup/)
[ ] Backup storage has sufficient free space (> 20%)
```

---

## 5. Contact Escalation Tree

### 5.1 Escalation Levels

| Level | Role | Contact | Response Time |
|-------|------|---------|---------------|
| L1 | On-Call Engineer | [On-call rotation — PagerDuty] | 15 min |
| L2 | Platform Lead | platform-lead@obss.example.com | 30 min |
| L3 | Infrastructure Lead | infra-lead@obss.example.com | 45 min |
| L4 | CTO / VP Engineering | cto@obss.example.com | 60 min |

### 5.2 Team Contacts

| Role | Name | Phone | Email |
|------|------|-------|-------|
| DR Lead | [Rotating Role] | [PagerDuty Number] | dr-lead@obss.example.com |
| Database Administrator | [DBA Name] | [Phone] | dba@obss.example.com |
| Security Lead | [Sec Name] | [Phone] | security@obss.example.com |
| Network Engineer | [Net Name] | [Phone] | network@obss.example.com |
| Application Support | [App Name] | [Phone] | appsupport@obss.example.com |
| Vendor Support (Keycloak) | [Vendor] | [Support #] | [Vendor Email] |
| Cloud Provider Support | [Cloud] | [Support #] | [Cloud Email] |

### 5.3 Communication Channels

| Channel | Purpose | Details |
|---------|---------|---------|
| **#dr-alerts** (Slack) | Incident notifications | Critical alerts |
| **#dr-coordination** (Slack) | Real-time coordination | During drill / incident |
| **PagerDuty** | Escalation | On-call rotation |
| **Email** | Post-incident report | dr-team@obss.example.com |
| **Phone bridge** | Voice coordination | [Bridge number and PIN] |

### 5.4 Escalation Procedure

1. **Alert fires** → PagerDuty notifies L1 on-call
2. **L1 acknowledges** within 15 min, or escalates to L2
3. **L1 triages** — if unable to resolve in 30 min, escalates to L2
4. **L2 leads** incident response, engages L3 if needed
5. **L3 engages** for infrastructure-level issues, vendor support
6. **L4 notified** if incident exceeds 2 hours or has business impact
7. **Post-incident** report filed within 48 hours

### 5.5 Vendor Support Contacts

| Vendor | Product | Support Level | Contact |
|--------|---------|---------------|---------|
| Red Hat | Keycloak | Production | [Support Portal] |
| OpenSearch | OpenSearch | Community / Enterprise | [Forum / Support] |
| Docker Inc. | Docker Engine | Business | [Support Portal] |
| [Cloud Provider] | IaaS | Enterprise | [Support Portal] |

---

## 6. Appendix

### 6.1 Backup Scripts Location

All backup and restore scripts are located in the repository under `/infrastructure/backup/`:

```
infrastructure/backup/
├── backup-scripts/
│   ├── postgres-backup.sh
│   ├── keycloak-backup.sh
│   └── opensearch-backup.sh
└── restore-scripts/
    └── postgres-restore.sh
```

### 6.2 Environment Variables Required

| Variable | Description | Source |
|----------|-------------|--------|
| `POSTGRES_USER` | PostgreSQL admin user | `.env` |
| `POSTGRES_PASSWORD` | PostgreSQL admin password | `.env` / secrets manager |
| `POSTGRES_DB` | PostgreSQL database name | `.env` |
| `POSTGRES_HOST` | PostgreSQL host | `.env` |
| `POSTGRES_PORT` | PostgreSQL port | `.env` |
| `KEYCLOAK_ADMIN` | Keycloak admin username | `.env` |
| `KEYCLOAK_ADMIN_PASSWORD` | Keycloak admin password | `.env` / secrets manager |
| `KEYCLOAK_HOST` | Keycloak host | `.env` |
| `OPENSEARCH_HOST` | OpenSearch host | `.env` |
| `OPENSEARCH_INITIAL_ADMIN_PASSWORD` | OpenSearch admin password | `.env` / secrets manager |
| `AWS_ACCESS_KEY_ID` | S3 access key | Secrets manager |
| `AWS_SECRET_ACCESS_KEY` | S3 secret key | Secrets manager |
| `S3_BACKUP_BUCKET` | S3 backup bucket name | `.env` |
| `GPG_PASSPHRASE` | GPG encryption passphrase | Secrets manager |
| `BACKUP_DIR` | Local backup staging directory | `.env` |
| `SLACK_WEBHOOK_URL` | Slack notification webhook | `.env` / secrets manager |
| `PAGERDUTY_ROUTING_KEY` | PagerDuty Events API key | Secrets manager |

### 6.3 Restoration Decision Matrix

| Situation | Recovery Method | Expected RTO |
|-----------|----------------|--------------|
| Accidental row deletion (recent) | Point-in-time recovery (WAL replay) | 30 min |
| Table dropped | Restore from daily dump + WAL replay | 45 min |
| Database corrupted | Restore from daily dump + WAL replay | 1 hour |
| Full host failure | Full site recovery to DR | 4 hours |
| Ransomware attack | Full site recovery from immutable backup | 4 hours |
| Bad deployment (code) | Rollback Docker image tag | 10 min |
| Bad migration (schema) | Restore from pre-migration dump | 30 min |
| Certificate expiry | Renew and redeploy | 15 min |
| Cloud region outage | Full site failover to secondary region | 4 hours |

### 6.4 Glossary

| Term | Definition |
|------|------------|
| **RTO** | Recovery Time Objective — maximum acceptable downtime |
| **RPO** | Recovery Point Objective — maximum acceptable data loss |
| **WAL** | Write-Ahead Log — PostgreSQL's transaction log for PITR |
| **PITR** | Point-in-Time Recovery — restoring to a specific moment |
| **DR** | Disaster Recovery |
| **S3** | Simple Storage Service (AWS-compatible object storage) |

### 6.5 Revision History

| Date | Version | Author | Changes |
|------|---------|--------|---------|
| 2026-06-21 | 1.0 | Platform Reliability Team | Initial DR runbook |

---

*End of Runbook*
