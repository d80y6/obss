# Performance Testing Infrastructure

This directory contains performance testing infrastructure for the Telecom OSS/BSS Platform. It includes BenchmarkDotNet unit benchmarks, k6 load/stress/spike tests, and Locust-based user simulation tests.

## Directory Structure

```
tests/Performance/
├── Benchmarks/
│   ├── IamBenchmarks.cs        # IAM domain benchmarks (user create, activate, roles)
│   ├── OrderBenchmarks.cs       # Order domain benchmarks (create, submit, approve, lifecycle)
│   └── BillingBenchmarks.cs     # Billing domain benchmarks (bill creation, calculation, finalization)
├── k6/
│   ├── load-test.js             # Multi-scenario load test (login, CRUD, orders, invoices)
│   ├── stress-test.js           # Ramp-up stress test (0 to 500 users)
│   └── spike-test.js            # Sudden spike test (0 to 1000 users in 30s)
├── Locust/
│   └── locustfile.py            # Weighted user-behavior simulation
├── Obss.Performance.Tests.csproj
└── README.md
```

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [k6](https://k6.io/docs/getting-started/installation/) v0.50+
- [Locust](https://docs.locust.io/en/stable/installation.html) 2.x+
- [Python 3.10+](https://www.python.org/) (for Locust)

## Running BenchmarkDotNet Tests

BenchmarkDotNet tests measure the performance of domain logic operations directly, without I/O dependencies. They test entity creation, state transitions, and calculations.

```bash
# Run all benchmarks
dotnet run --project tests/Performance/Obss.Performance.Tests.csproj -c Release

# Run specific benchmark class
dotnet run --project tests/Performance/Obss.Performance.Tests.csproj -c Release -- --filter *IamBenchmarks*

# Run with specific job configuration
dotnet run --project tests/Performance/Obss.Performance.Tests.csproj -c Release -- --job short
```

BenchmarkDotNet will output results to the console and save detailed reports in `BenchmarkDotNet.Artifacts/results/`.

### Benchmark Categories

| Benchmark | Domain | Operations Tested |
|-----------|--------|-------------------|
| IamBenchmarks | IAM | User create, activate, deactivate, role assignment, profile update, email verification |
| OrderBenchmarks | Orders | Order create, add items, submit, approve, full lifecycle, cancel, add/remove items |
| BillingBenchmarks | Billing | Bill creation, line generation, totals calculation, tax recalculation, finalization |

## Running k6 Tests

### Load Test

Simulates 4 concurrent scenarios: 100 users logging in, 50 performing customer CRUD, 30 creating orders, and 20 generating invoices.

```bash
# Default (pointing to localhost:5000)
k6 run tests/Performance/k6/load-test.js

# Custom target URL
k6 run -e BASE_URL=https://staging.obss.example.com tests/Performance/k6/load-test.js

# With output to JSON
k6 run --out json=results/load-test.json tests/Performance/k6/load-test.js
```

**Thresholds:**
- p95 response time < 500ms for all scenarios
- Error rate < 1%

### Stress Test

Ramps from 0 to 500 concurrent users over 5 minutes, holds for 10 minutes, then ramps down.

```bash
k6 run tests/Performance/k6/stress-test.js

# With HTML summary report
k6 run --out html=results/stress-report.html tests/Performance/k6/stress-test.js
```

**Thresholds:**
- p95 < 2000ms, p99 < 5000ms
- Error rate < 5%

### Spike Test

Spikes from 0 to 1000 users in 30 seconds, holds for 2 minutes, then observes recovery.

```bash
k6 run tests/Performance/k6/spike-test.js

# With granular output
k6 run --out json=results/spike-test.json tests/Performance/k6/spike-test.js
```

**Thresholds:**
- p95 < 3000ms, p99 < 8000ms
- Error rate < 10%
- Throughput > 100 requests/second (minimum)

## Running Locust Tests

Locust provides a web UI for real-time monitoring and a headless mode for CI.

```bash
# Web UI mode (http://localhost:8089)
locust -f tests/Performance/Locust/locustfile.py --host=http://localhost:5000

# Headless mode
locust -f tests/Performance/Locust/locustfile.py \
  --host=http://localhost:5000 \
  --headless \
  --users 200 \
  --spawn-rate 10 \
  --run-time 10m \
  --csv=results/locust-report

# Using Locust with custom shape class
locust -f tests/Performance/Locust/locustfile.py \
  --host=http://localhost:5000 \
  --headless \
  --users 500 \
  --spawn-rate 20 \
  --run-time 15m
```

### User Weights

| User Class | Weight | Simulated Behavior |
|------------|--------|-------------------|
| IamUser | 3 | User CRUD and authentication flows |
| CrmUser | 2 | Customer CRUD and search |
| OrderUser | 2 | Order creation, submission, approval |
| BillingUser | 1 | Bill generation, tax calculation, finalization |

## Interpreting Results

### BenchmarkDotNet Output

Key metrics per benchmark:
- **Mean**: Average execution time
- **Median**: Median execution time (less sensitive to outliers)
- **Min/Max**: Range of observed times
- **Allocated**: Memory allocated per operation (important for GC pressure analysis)

Compare results against baselines stored in `docs/performance-baselines.md`.

### k6 Results

Key metrics reported by k6:
- **http_req_duration**: Request latency distribution (p50, p90, p95, p99)
- **http_req_failed**: Percentage of failed requests
- **http_reqs**: Throughput in requests per second
- **vus**: Number of concurrent virtual users
- **iteration_duration**: End-to-end time for complete scenario iterations

### Locust Results

Locust provides:
- **Request statistics**: Per-endpoint latency distribution
- **Failures**: Detailed error breakdown by endpoint
- **Response time chart**: Real-time visualization in web UI
- **CSV export**: For post-processing and dashboards

## Performance Baselines

| Operation | Target (p95) | Target (mean) |
|-----------|-------------|---------------|
| User creation | < 500ms | < 200ms |
| User authentication | < 500ms | < 200ms |
| Customer CRUD | < 500ms | < 150ms |
| Order creation | < 800ms | < 300ms |
| Order processing (submit+approve) | < 1000ms | < 400ms |
| Bill generation | < 1000ms | < 400ms |
| Bill finalization | < 500ms | < 200ms |
| List queries | < 300ms | < 100ms |

Update baseline values in `docs/performance-baselines.md` as the system evolves.

## CI Integration

Add to your CI pipeline:

```yaml
# GitHub Actions example
- name: Run k6 load test
  run: |
    k6 run \
      --out json=${{ runner.temp }}/k6-results.json \
      tests/Performance/k6/load-test.js

- name: Check thresholds
  run: |
    # Parse k6 JSON output and fail if thresholds exceeded
    jq -e '.metrics.http_req_duration.p95 < 500' ${{ runner.temp }}/k6-results.json
```
