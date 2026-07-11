FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG BUILD_VERSION=1.0.0

WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["NuGet.config", "."]

COPY ["src/BuildingBlocks/Obss.ModuleRegistration/Obss.ModuleRegistration.csproj", "src/BuildingBlocks/Obss.ModuleRegistration/"]
COPY ["src/Shared/Obss.SharedKernel/Obss.SharedKernel.csproj", "src/Shared/Obss.SharedKernel/"]

COPY ["src/Modules/IAM/Obss.IAM.Domain/Obss.IAM.Domain.csproj", "src/Modules/IAM/Obss.IAM.Domain/"]
COPY ["src/Modules/IAM/Obss.IAM.Application/Obss.IAM.Application.csproj", "src/Modules/IAM/Obss.IAM.Application/"]
COPY ["src/Modules/IAM/Obss.IAM.Infrastructure/Obss.IAM.Infrastructure.csproj", "src/Modules/IAM/Obss.IAM.Infrastructure/"]
COPY ["src/Modules/IAM/Obss.IAM.Api/Obss.IAM.Api.csproj", "src/Modules/IAM/Obss.IAM.Api/"]

COPY ["src/Modules/CRM/Obss.CRM.Domain/Obss.CRM.Domain.csproj", "src/Modules/CRM/Obss.CRM.Domain/"]
COPY ["src/Modules/CRM/Obss.CRM.Application/Obss.CRM.Application.csproj", "src/Modules/CRM/Obss.CRM.Application/"]
COPY ["src/Modules/CRM/Obss.CRM.Infrastructure/Obss.CRM.Infrastructure.csproj", "src/Modules/CRM/Obss.CRM.Infrastructure/"]
COPY ["src/Modules/CRM/Obss.CRM.Api/Obss.CRM.Api.csproj", "src/Modules/CRM/Obss.CRM.Api/"]

COPY ["src/Modules/ProductCatalog/Obss.ProductCatalog.Domain/Obss.ProductCatalog.Domain.csproj", "src/Modules/ProductCatalog/Obss.ProductCatalog.Domain/"]
COPY ["src/Modules/ProductCatalog/Obss.ProductCatalog.Application/Obss.ProductCatalog.Application.csproj", "src/Modules/ProductCatalog/Obss.ProductCatalog.Application/"]
COPY ["src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure/Obss.ProductCatalog.Infrastructure.csproj", "src/Modules/ProductCatalog/Obss.ProductCatalog.Infrastructure/"]
COPY ["src/Modules/ProductCatalog/Obss.ProductCatalog.Api/Obss.ProductCatalog.Api.csproj", "src/Modules/ProductCatalog/Obss.ProductCatalog.Api/"]

COPY ["src/Modules/Orders/Obss.Orders.Domain/Obss.Orders.Domain.csproj", "src/Modules/Orders/Obss.Orders.Domain/"]
COPY ["src/Modules/Orders/Obss.Orders.Application/Obss.Orders.Application.csproj", "src/Modules/Orders/Obss.Orders.Application/"]
COPY ["src/Modules/Orders/Obss.Orders.Infrastructure/Obss.Orders.Infrastructure.csproj", "src/Modules/Orders/Obss.Orders.Infrastructure/"]
COPY ["src/Modules/Orders/Obss.Orders.Api/Obss.Orders.Api.csproj", "src/Modules/Orders/Obss.Orders.Api/"]

COPY ["src/Modules/Subscriptions/Obss.Subscriptions.Domain/Obss.Subscriptions.Domain.csproj", "src/Modules/Subscriptions/Obss.Subscriptions.Domain/"]
COPY ["src/Modules/Subscriptions/Obss.Subscriptions.Application/Obss.Subscriptions.Application.csproj", "src/Modules/Subscriptions/Obss.Subscriptions.Application/"]
COPY ["src/Modules/Subscriptions/Obss.Subscriptions.Infrastructure/Obss.Subscriptions.Infrastructure.csproj", "src/Modules/Subscriptions/Obss.Subscriptions.Infrastructure/"]
COPY ["src/Modules/Subscriptions/Obss.Subscriptions.Api/Obss.Subscriptions.Api.csproj", "src/Modules/Subscriptions/Obss.Subscriptions.Api/"]

COPY ["src/Modules/Billing/Obss.Billing.Domain/Obss.Billing.Domain.csproj", "src/Modules/Billing/Obss.Billing.Domain/"]
COPY ["src/Modules/Billing/Obss.Billing.Application/Obss.Billing.Application.csproj", "src/Modules/Billing/Obss.Billing.Application/"]
COPY ["src/Modules/Billing/Obss.Billing.Infrastructure/Obss.Billing.Infrastructure.csproj", "src/Modules/Billing/Obss.Billing.Infrastructure/"]
COPY ["src/Modules/Billing/Obss.Billing.Api/Obss.Billing.Api.csproj", "src/Modules/Billing/Obss.Billing.Api/"]

COPY ["src/Modules/Invoices/Obss.Invoices.Domain/Obss.Invoices.Domain.csproj", "src/Modules/Invoices/Obss.Invoices.Domain/"]
COPY ["src/Modules/Invoices/Obss.Invoices.Application/Obss.Invoices.Application.csproj", "src/Modules/Invoices/Obss.Invoices.Application/"]
COPY ["src/Modules/Invoices/Obss.Invoices.Infrastructure/Obss.Invoices.Infrastructure.csproj", "src/Modules/Invoices/Obss.Invoices.Infrastructure/"]
COPY ["src/Modules/Invoices/Obss.Invoices.Api/Obss.Invoices.Api.csproj", "src/Modules/Invoices/Obss.Invoices.Api/"]

COPY ["src/Modules/Payments/Obss.Payments.Domain/Obss.Payments.Domain.csproj", "src/Modules/Payments/Obss.Payments.Domain/"]
COPY ["src/Modules/Payments/Obss.Payments.Application/Obss.Payments.Application.csproj", "src/Modules/Payments/Obss.Payments.Application/"]
COPY ["src/Modules/Payments/Obss.Payments.Infrastructure/Obss.Payments.Infrastructure.csproj", "src/Modules/Payments/Obss.Payments.Infrastructure/"]
COPY ["src/Modules/Payments/Obss.Payments.Api/Obss.Payments.Api.csproj", "src/Modules/Payments/Obss.Payments.Api/"]

COPY ["src/Modules/Rating/Obss.Rating.Domain/Obss.Rating.Domain.csproj", "src/Modules/Rating/Obss.Rating.Domain/"]
COPY ["src/Modules/Rating/Obss.Rating.Application/Obss.Rating.Application.csproj", "src/Modules/Rating/Obss.Rating.Application/"]
COPY ["src/Modules/Rating/Obss.Rating.Infrastructure/Obss.Rating.Infrastructure.csproj", "src/Modules/Rating/Obss.Rating.Infrastructure/"]
COPY ["src/Modules/Rating/Obss.Rating.Api/Obss.Rating.Api.csproj", "src/Modules/Rating/Obss.Rating.Api/"]

COPY ["src/Modules/Collections/Obss.Collections.Domain/Obss.Collections.Domain.csproj", "src/Modules/Collections/Obss.Collections.Domain/"]
COPY ["src/Modules/Collections/Obss.Collections.Application/Obss.Collections.Application.csproj", "src/Modules/Collections/Obss.Collections.Application/"]
COPY ["src/Modules/Collections/Obss.Collections.Infrastructure/Obss.Collections.Infrastructure.csproj", "src/Modules/Collections/Obss.Collections.Infrastructure/"]
COPY ["src/Modules/Collections/Obss.Collections.Api/Obss.Collections.Api.csproj", "src/Modules/Collections/Obss.Collections.Api/"]

COPY ["src/Modules/ServiceInventory/Obss.ServiceInventory.Domain/Obss.ServiceInventory.Domain.csproj", "src/Modules/ServiceInventory/Obss.ServiceInventory.Domain/"]
COPY ["src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Obss.ServiceInventory.Application.csproj", "src/Modules/ServiceInventory/Obss.ServiceInventory.Application/"]
COPY ["src/Modules/ServiceInventory/Obss.ServiceInventory.Infrastructure/Obss.ServiceInventory.Infrastructure.csproj", "src/Modules/ServiceInventory/Obss.ServiceInventory.Infrastructure/"]
COPY ["src/Modules/ServiceInventory/Obss.ServiceInventory.Api/Obss.ServiceInventory.Api.csproj", "src/Modules/ServiceInventory/Obss.ServiceInventory.Api/"]

COPY ["src/Modules/NetworkInventory/Obss.NetworkInventory.Domain/Obss.NetworkInventory.Domain.csproj", "src/Modules/NetworkInventory/Obss.NetworkInventory.Domain/"]
COPY ["src/Modules/NetworkInventory/Obss.NetworkInventory.Application/Obss.NetworkInventory.Application.csproj", "src/Modules/NetworkInventory/Obss.NetworkInventory.Application/"]
COPY ["src/Modules/NetworkInventory/Obss.NetworkInventory.Infrastructure/Obss.NetworkInventory.Infrastructure.csproj", "src/Modules/NetworkInventory/Obss.NetworkInventory.Infrastructure/"]
COPY ["src/Modules/NetworkInventory/Obss.NetworkInventory.Api/Obss.NetworkInventory.Api.csproj", "src/Modules/NetworkInventory/Obss.NetworkInventory.Api/"]

COPY ["src/Modules/Provisioning/Obss.Provisioning.Domain/Obss.Provisioning.Domain.csproj", "src/Modules/Provisioning/Obss.Provisioning.Domain/"]
COPY ["src/Modules/Provisioning/Obss.Provisioning.Application/Obss.Provisioning.Application.csproj", "src/Modules/Provisioning/Obss.Provisioning.Application/"]
COPY ["src/Modules/Provisioning/Obss.Provisioning.Infrastructure/Obss.Provisioning.Infrastructure.csproj", "src/Modules/Provisioning/Obss.Provisioning.Infrastructure/"]
COPY ["src/Modules/Provisioning/Obss.Provisioning.Api/Obss.Provisioning.Api.csproj", "src/Modules/Provisioning/Obss.Provisioning.Api/"]

COPY ["src/Modules/Workflow/Obss.Workflow.Domain/Obss.Workflow.Domain.csproj", "src/Modules/Workflow/Obss.Workflow.Domain/"]
COPY ["src/Modules/Workflow/Obss.Workflow.Application/Obss.Workflow.Application.csproj", "src/Modules/Workflow/Obss.Workflow.Application/"]
COPY ["src/Modules/Workflow/Obss.Workflow.Infrastructure/Obss.Workflow.Infrastructure.csproj", "src/Modules/Workflow/Obss.Workflow.Infrastructure/"]
COPY ["src/Modules/Workflow/Obss.Workflow.Api/Obss.Workflow.Api.csproj", "src/Modules/Workflow/Obss.Workflow.Api/"]

COPY ["src/Modules/Ticketing/Obss.Ticketing.Domain/Obss.Ticketing.Domain.csproj", "src/Modules/Ticketing/Obss.Ticketing.Domain/"]
COPY ["src/Modules/Ticketing/Obss.Ticketing.Application/Obss.Ticketing.Application.csproj", "src/Modules/Ticketing/Obss.Ticketing.Application/"]
COPY ["src/Modules/Ticketing/Obss.Ticketing.Infrastructure/Obss.Ticketing.Infrastructure.csproj", "src/Modules/Ticketing/Obss.Ticketing.Infrastructure/"]
COPY ["src/Modules/Ticketing/Obss.Ticketing.Api/Obss.Ticketing.Api.csproj", "src/Modules/Ticketing/Obss.Ticketing.Api/"]

COPY ["src/Modules/Notifications/Obss.Notifications.Domain/Obss.Notifications.Domain.csproj", "src/Modules/Notifications/Obss.Notifications.Domain/"]
COPY ["src/Modules/Notifications/Obss.Notifications.Application/Obss.Notifications.Application.csproj", "src/Modules/Notifications/Obss.Notifications.Application/"]
COPY ["src/Modules/Notifications/Obss.Notifications.Infrastructure/Obss.Notifications.Infrastructure.csproj", "src/Modules/Notifications/Obss.Notifications.Infrastructure/"]
COPY ["src/Modules/Notifications/Obss.Notifications.Api/Obss.Notifications.Api.csproj", "src/Modules/Notifications/Obss.Notifications.Api/"]

COPY ["src/Modules/Reporting/Obss.Reporting.Domain/Obss.Reporting.Domain.csproj", "src/Modules/Reporting/Obss.Reporting.Domain/"]
COPY ["src/Modules/Reporting/Obss.Reporting.Application/Obss.Reporting.Application.csproj", "src/Modules/Reporting/Obss.Reporting.Application/"]
COPY ["src/Modules/Reporting/Obss.Reporting.Infrastructure/Obss.Reporting.Infrastructure.csproj", "src/Modules/Reporting/Obss.Reporting.Infrastructure/"]
COPY ["src/Modules/Reporting/Obss.Reporting.Api/Obss.Reporting.Api.csproj", "src/Modules/Reporting/Obss.Reporting.Api/"]

COPY ["src/Modules/Audit/Obss.Audit.Domain/Obss.Audit.Domain.csproj", "src/Modules/Audit/Obss.Audit.Domain/"]
COPY ["src/Modules/Audit/Obss.Audit.Application/Obss.Audit.Application.csproj", "src/Modules/Audit/Obss.Audit.Application/"]
COPY ["src/Modules/Audit/Obss.Audit.Infrastructure/Obss.Audit.Infrastructure.csproj", "src/Modules/Audit/Obss.Audit.Infrastructure/"]
COPY ["src/Modules/Audit/Obss.Audit.Api/Obss.Audit.Api.csproj", "src/Modules/Audit/Obss.Audit.Api/"]

COPY ["src/Modules/ApiGateway/Obss.ApiGateway.Domain/Obss.ApiGateway.Domain.csproj", "src/Modules/ApiGateway/Obss.ApiGateway.Domain/"]
COPY ["src/Modules/ApiGateway/Obss.ApiGateway.Application/Obss.ApiGateway.Application.csproj", "src/Modules/ApiGateway/Obss.ApiGateway.Application/"]
COPY ["src/Modules/ApiGateway/Obss.ApiGateway.Infrastructure/Obss.ApiGateway.Infrastructure.csproj", "src/Modules/ApiGateway/Obss.ApiGateway.Infrastructure/"]
COPY ["src/Modules/ApiGateway/Obss.ApiGateway.Api/Obss.ApiGateway.Api.csproj", "src/Modules/ApiGateway/Obss.ApiGateway.Api/"]

COPY ["src/Modules/NumberInventory/Obss.NumberInventory.Domain/Obss.NumberInventory.Domain.csproj", "src/Modules/NumberInventory/Obss.NumberInventory.Domain/"]
COPY ["src/Modules/NumberInventory/Obss.NumberInventory.Application/Obss.NumberInventory.Application.csproj", "src/Modules/NumberInventory/Obss.NumberInventory.Application/"]
COPY ["src/Modules/NumberInventory/Obss.NumberInventory.Infrastructure/Obss.NumberInventory.Infrastructure.csproj", "src/Modules/NumberInventory/Obss.NumberInventory.Infrastructure/"]
COPY ["src/Modules/NumberInventory/Obss.NumberInventory.Api/Obss.NumberInventory.Api.csproj", "src/Modules/NumberInventory/Obss.NumberInventory.Api/"]
COPY ["src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/Obss.ServiceCatalog.Domain.csproj", "src/Modules/ServiceCatalog/Obss.ServiceCatalog.Domain/"]
COPY ["src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/Obss.ServiceCatalog.Application.csproj", "src/Modules/ServiceCatalog/Obss.ServiceCatalog.Application/"]
COPY ["src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure/Obss.ServiceCatalog.Infrastructure.csproj", "src/Modules/ServiceCatalog/Obss.ServiceCatalog.Infrastructure/"]
COPY ["src/Modules/ServiceCatalog/Obss.ServiceCatalog.Api/Obss.ServiceCatalog.Api.csproj", "src/Modules/ServiceCatalog/Obss.ServiceCatalog.Api/"]
COPY ["src/Modules/EventManagement/Obss.EventManagement.Domain/Obss.EventManagement.Domain.csproj", "src/Modules/EventManagement/Obss.EventManagement.Domain/"]
COPY ["src/Modules/EventManagement/Obss.EventManagement.Application/Obss.EventManagement.Application.csproj", "src/Modules/EventManagement/Obss.EventManagement.Application/"]
COPY ["src/Modules/EventManagement/Obss.EventManagement.Infrastructure/Obss.EventManagement.Infrastructure.csproj", "src/Modules/EventManagement/Obss.EventManagement.Infrastructure/"]
COPY ["src/Modules/EventManagement/Obss.EventManagement.Api/Obss.EventManagement.Api.csproj", "src/Modules/EventManagement/Obss.EventManagement.Api/"]
COPY ["src/Modules/ServiceQualification/Obss.ServiceQualification.Domain/Obss.ServiceQualification.Domain.csproj", "src/Modules/ServiceQualification/Obss.ServiceQualification.Domain/"]
COPY ["src/Modules/ServiceQualification/Obss.ServiceQualification.Application/Obss.ServiceQualification.Application.csproj", "src/Modules/ServiceQualification/Obss.ServiceQualification.Application/"]
COPY ["src/Modules/ServiceQualification/Obss.ServiceQualification.Infrastructure/Obss.ServiceQualification.Infrastructure.csproj", "src/Modules/ServiceQualification/Obss.ServiceQualification.Infrastructure/"]
COPY ["src/Modules/ServiceQualification/Obss.ServiceQualification.Api/Obss.ServiceQualification.Api.csproj", "src/Modules/ServiceQualification/Obss.ServiceQualification.Api/"]

COPY ["src/Host/Obss.Host/Obss.Host.csproj", "src/Host/Obss.Host/"]

RUN dotnet restore "src/Host/Obss.Host/Obss.Host.csproj"

COPY ["src/BuildingBlocks/Obss.ModuleRegistration/", "src/BuildingBlocks/Obss.ModuleRegistration/"]
COPY ["src/Shared/Obss.SharedKernel/", "src/Shared/Obss.SharedKernel/"]
COPY ["src/Modules/IAM/", "src/Modules/IAM/"]
COPY ["src/Modules/CRM/", "src/Modules/CRM/"]
COPY ["src/Modules/ProductCatalog/", "src/Modules/ProductCatalog/"]
COPY ["src/Modules/Orders/", "src/Modules/Orders/"]
COPY ["src/Modules/Subscriptions/", "src/Modules/Subscriptions/"]
COPY ["src/Modules/Billing/", "src/Modules/Billing/"]
COPY ["src/Modules/Invoices/", "src/Modules/Invoices/"]
COPY ["src/Modules/Payments/", "src/Modules/Payments/"]
COPY ["src/Modules/Rating/", "src/Modules/Rating/"]
COPY ["src/Modules/Collections/", "src/Modules/Collections/"]
COPY ["src/Modules/ServiceInventory/", "src/Modules/ServiceInventory/"]
COPY ["src/Modules/NetworkInventory/", "src/Modules/NetworkInventory/"]
COPY ["src/Modules/Provisioning/", "src/Modules/Provisioning/"]
COPY ["src/Modules/Workflow/", "src/Modules/Workflow/"]
COPY ["src/Modules/Ticketing/", "src/Modules/Ticketing/"]
COPY ["src/Modules/Notifications/", "src/Modules/Notifications/"]
COPY ["src/Modules/Reporting/", "src/Modules/Reporting/"]
COPY ["src/Modules/Audit/", "src/Modules/Audit/"]
COPY ["src/Modules/ApiGateway/", "src/Modules/ApiGateway/"]
COPY ["src/Modules/NumberInventory/", "src/Modules/NumberInventory/"]
COPY ["src/Modules/ServiceCatalog/", "src/Modules/ServiceCatalog/"]
COPY ["src/Modules/EventManagement/", "src/Modules/EventManagement/"]
COPY ["src/Modules/ServiceQualification/", "src/Modules/ServiceQualification/"]
COPY ["src/Host/Obss.Host/", "src/Host/Obss.Host/"]

RUN dotnet publish "src/Host/Obss.Host/Obss.Host.csproj" \
    --configuration ${BUILD_CONFIGURATION} \
    --no-restore \
    --output /app/publish \
    -p:Version=${BUILD_VERSION} \
    -p:ContinuousIntegrationBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
ARG BUILD_VERSION=1.0.0

RUN addgroup --system --gid 1000 obss \
    && adduser --system --uid 1000 --ingroup obss --no-create-home --disabled-password obss

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build --chown=obss:obss /app/publish .

LABEL org.opencontainers.image.title="OBSS Platform Host" \
      org.opencontainers.image.description="Telecom OSS/BSS Platform - Modular Monolith Host" \
      org.opencontainers.image.version="${BUILD_VERSION}" \
      org.opencontainers.image.vendor="OBSS" \
      org.opencontainers.image.licenses="MIT" \
      org.opencontainers.image.created="$(date -u +'%Y-%m-%dT%H:%M:%SZ')" \
      org.opencontainers.image.documentation="https://github.com/obss/platform" \
      com.obss.platform.component="obss-host" \
      com.obss.platform.otel-enabled="true"

USER obss

HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Obss.Host.dll"]
