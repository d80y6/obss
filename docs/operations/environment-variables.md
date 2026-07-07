# Environment Variables

The OBSS platform uses environment variables for all sensitive configuration. The `appsettings.json` file contains non-sensitive defaults; credentials **must** be provided via environment variables.

## Required Environment Variables

### PostgreSQL

| Variable | Description | Default |
|----------|-------------|---------|
| `POSTGRES_CONNECTION_STRING` | Full PostgreSQL connection string (takes highest priority) | — |
| `POSTGRES_PASSWORD` | PostgreSQL password (appended to connection string from `appsettings.json`) | — |

If `POSTGRES_CONNECTION_STRING` is set, it is used as-is. Otherwise, the connection string is built from `ConnectionStrings:DefaultConnection` in configuration with the password appended from `POSTGRES_PASSWORD`.

**Example:**
```bash
export POSTGRES_CONNECTION_STRING="Host=localhost;Port=5432;Database=obss;Username=obss_admin;Password=your_password"
```

### Redis

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__Redis` | Redis connection string with optional password | `localhost:6379` |

**Example:**
```bash
export ConnectionStrings__Redis="localhost:6379,password=your_password"
```

### RabbitMQ

| Variable | Description | Default |
|----------|-------------|---------|
| `RabbitMq__Host` | RabbitMQ host | `localhost` |
| `RabbitMq__Port` | RabbitMQ port | `5672` |
| `RabbitMq__Username` | RabbitMQ username | `obss_admin` |
| `RabbitMq__Password` | RabbitMQ password | (empty) |
| `RabbitMq__VirtualHost` | RabbitMQ virtual host | `/` |

**Example:**
```bash
export RabbitMq__Password="your_rabbitmq_password"
```

### Keycloak

| Variable | Description | Default |
|----------|-------------|---------|
| `Keycloak__Authority` | Keycloak realm authority URL | `http://localhost:8080/realms/obss` |
| `Keycloak__Audience` | Expected JWT audience | `obss-api` |
| `Keycloak__ClientSecret` | Keycloak client secret | (empty) |

### OpenTelemetry

| Variable | Description | Default |
|----------|-------------|---------|
| `OpenTelemetry__Endpoint` | OTLP exporter endpoint | `http://localhost:4317` |
| `OpenTelemetry__ServiceName` | Service name for traces/metrics | `Obss.Host` |

### Environment-Specific Config

For **development**, use either:
1. `appsettings.Development.json` overrides
2. `dotnet user-secrets` (`dotnet user-secrets set "RabbitMq:Password" "value"`)
3. Environment variables as listed above

For **production**, use environment variables exclusively (never store credentials in source-controlled config files).

## Configuration Precedence

1. Environment variables (highest)
2. User secrets (Development only)
3. `appsettings.{Environment}.json`
4. `appsettings.json` (lowest)
