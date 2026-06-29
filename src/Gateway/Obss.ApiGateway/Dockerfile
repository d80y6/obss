# =============================================================================
# OBSS Platform - API Gateway Dockerfile
# Telecom OSS/BSS Platform
# =============================================================================

# ── Base image (SDK) ───────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG BUILD_VERSION=1.0.0

WORKDIR /src

# Copy project files for layer caching
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["NuGet.config", "."]

# Copy shared kernel
COPY ["src/Shared/Obss.SharedKernel/Obss.SharedKernel.csproj", "src/Shared/Obss.SharedKernel/"]

# Copy ApiGateway projects
COPY ["src/Modules/ApiGateway/Obss.ApiGateway.Domain/Obss.ApiGateway.Domain.csproj", "src/Modules/ApiGateway/Obss.ApiGateway.Domain/"]
COPY ["src/Modules/ApiGateway/Obss.ApiGateway.Application/Obss.ApiGateway.Application.csproj", "src/Modules/ApiGateway/Obss.ApiGateway.Application/"]
COPY ["src/Modules/ApiGateway/Obss.ApiGateway.Infrastructure/Obss.ApiGateway.Infrastructure.csproj", "src/Modules/ApiGateway/Obss.ApiGateway.Infrastructure/"]

# Restore dependencies (layer caching)
RUN dotnet restore "src/Modules/ApiGateway/Obss.ApiGateway.Infrastructure/Obss.ApiGateway.Infrastructure.csproj"

# Copy source files
COPY ["src/Shared/Obss.SharedKernel/", "src/Shared/Obss.SharedKernel/"]
COPY ["src/Modules/ApiGateway/Obss.ApiGateway.Domain/", "src/Modules/ApiGateway/Obss.ApiGateway.Domain/"]
COPY ["src/Modules/ApiGateway/Obss.ApiGateway.Application/", "src/Modules/ApiGateway/Obss.ApiGateway.Application/"]
COPY ["src/Modules/ApiGateway/Obss.ApiGateway.Infrastructure/", "src/Modules/ApiGateway/Obss.ApiGateway.Infrastructure/"]

# Build and publish
RUN dotnet publish "src/Modules/ApiGateway/Obss.ApiGateway.Infrastructure/Obss.ApiGateway.Infrastructure.csproj" \
    --configuration ${BUILD_CONFIGURATION} \
    --no-restore \
    --output /app/publish \
    -p:Version=${BUILD_VERSION} \
    -p:ContinuousIntegrationBuild=true

# ── Runtime image ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
ARG BUILD_VERSION=1.0.0

# Create non-root user
RUN addgroup --system --gid 1000 obss \
    && adduser --system --uid 1000 --ingroup obss --no-create-home --disabled-password obss

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install curl for health checks
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Copy published artifacts
COPY --from=build --chown=obss:obss /app/publish .

# Set labels
LABEL org.opencontainers.image.title="OBSS API Gateway" \
      org.opencontainers.image.description="Telecom OSS/BSS Platform - API Gateway" \
      org.opencontainers.image.version="${BUILD_VERSION}" \
      org.opencontainers.image.vendor="OBSS" \
      org.opencontainers.image.licenses="MIT" \
      org.opencontainers.image.created="$(date -u +'%Y-%m-%dT%H:%M:%SZ')" \
      org.opencontainers.image.documentation="https://github.com/obss/platform" \
      com.obss.platform.component="api-gateway" \
      com.obss.platform.otel-enabled="true"

# Switch to non-root user
USER obss

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Obss.ApiGateway.Infrastructure.dll"]
