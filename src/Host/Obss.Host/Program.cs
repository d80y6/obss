using System.Reflection;
using System.Text.Json.Serialization;
using Asp.Versioning;
using EFCore.NamingConventions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Obss.ApiGateway.Api.Extensions;
using Obss.Audit.Api.Extensions;
using Obss.Billing.Api.Extensions;
using Obss.CRM.Api.Extensions;
using Obss.Collections.Api.Extensions;
using Obss.Host.Middleware;
using Obss.IAM.Api.Extensions;
using Obss.Invoices.Api.Extensions;
using Obss.NetworkInventory.Api.Extensions;
using Obss.NumberInventory.Api.Extensions;
using Obss.Notifications.Api.Extensions;
using Obss.Orders.Api.Extensions;
using Obss.Payments.Api.Extensions;
using Obss.ProductCatalog.Api.Extensions;
using Obss.Provisioning.Api.Extensions;
using Obss.Rating.Api.Extensions;
using Obss.Reporting.Api.Extensions;
using Obss.ServiceCatalog.Api.Extensions;
using Obss.EventManagement.Api.Extensions;
using Obss.ServiceInventory.Api.Extensions;
using Obss.ServiceQualification.Api.Extensions;
using Obss.AAA.Api.Extensions;
using Obss.OCS.Api.Extensions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Authorization;
using Obss.SharedKernel.Application.Behaviors;
using Obss.SharedKernel.Infrastructure;
using Obss.SharedKernel.Infrastructure.Authorization;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.Subscriptions.Api.Extensions;
using Obss.Ticketing.Api.Extensions;
using Obss.Workflow.Api.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

#pragma warning disable S3903 // Types in top-level statement files are allowed for minimal API records

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = Environments.Development
});
builder.Host.UseDefaultServiceProvider((context, options) =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = false;
});

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer((schema, context, cancellationToken) =>
    {
        if (schema.Enum?.Count > 0 && schema.Type == "integer")
        {
            schema.Type = "string";
        }

        if (schema.Required?.Count > 0 && schema.Properties is not null)
        {
            var nullableRequired = schema.Properties
                .Where(p => p.Value?.Nullable == true)
                .Select(p => p.Key)
                .ToHashSet();

            if (nullableRequired.Count > 0)
            {
                schema.Required = schema.Required.Where(r => !nullableRequired.Contains(r)).ToHashSet();
                if (schema.Required.Count == 0)
                    schema.Required = null;
            }
        }

        return Task.CompletedTask;
    });
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
        if (!allowedOrigins.Contains("*"))
            policy.AllowCredentials();
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keycloakSection = builder.Configuration.GetSection("Keycloak");
        var authority = keycloakSection["Authority"] ?? throw new InvalidOperationException("Keycloak:Authority is required");
        options.Authority = authority;
        options.Audience = keycloakSection["Audience"];
        options.RequireHttpsMetadata = bool.TryParse(keycloakSection["RequireHttpsMetadata"], out var https) && https;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authority,
            ValidateAudience = true,
            ValidAudience = keycloakSection["Audience"],
            ValidateLifetime = true,
            RoleClaimType = "role",
            NameClaimType = "preferred_username"
        };
        options.Backchannel = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
    });

builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    void AddPermissionPolicy(string permission)
    {
        options.AddPolicy(Permissions.PolicyName(permission), policy =>
            policy.RequireAuthenticatedUser()
                  .AddRequirements(new PermissionRequirement(permission)));
    }

    AddPermissionPolicy(Permissions.Iam.Platform);
    AddPermissionPolicy(Permissions.Iam.TenantRead);
    AddPermissionPolicy(Permissions.Iam.TenantWrite);
    AddPermissionPolicy(Permissions.Iam.UserRead);
    AddPermissionPolicy(Permissions.Iam.UserWrite);
    AddPermissionPolicy(Permissions.Iam.UserDeactivate);
    AddPermissionPolicy(Permissions.Iam.RoleRead);
    AddPermissionPolicy(Permissions.Iam.RoleWrite);
    AddPermissionPolicy(Permissions.Iam.PermissionManage);

    AddPermissionPolicy(Permissions.Billing.BillRead);
    AddPermissionPolicy(Permissions.Billing.BillWrite);
    AddPermissionPolicy(Permissions.Billing.BillFinalize);
    AddPermissionPolicy(Permissions.Billing.BillAdjust);
    AddPermissionPolicy(Permissions.Billing.AccountRead);
    AddPermissionPolicy(Permissions.Billing.AccountWrite);
    AddPermissionPolicy(Permissions.Billing.TaxManage);
    AddPermissionPolicy(Permissions.Billing.CycleManage);

    AddPermissionPolicy(Permissions.Payments.PaymentRead);
    AddPermissionPolicy(Permissions.Payments.PaymentWrite);
    AddPermissionPolicy(Permissions.Payments.PaymentRefund);
    AddPermissionPolicy(Permissions.Payments.PaymentGatewayManage);
    AddPermissionPolicy(Permissions.Payments.PaymentReconciliation);

    AddPermissionPolicy(Permissions.Invoices.InvoiceRead);
    AddPermissionPolicy(Permissions.Invoices.InvoiceWrite);
    AddPermissionPolicy(Permissions.Invoices.InvoiceFinalize);
    AddPermissionPolicy(Permissions.Invoices.InvoiceSend);
    AddPermissionPolicy(Permissions.Invoices.InvoiceCreditNote);
    AddPermissionPolicy(Permissions.Invoices.InvoiceDisputeManage);

    AddPermissionPolicy(Permissions.Provisioning.JobRead);
    AddPermissionPolicy(Permissions.Provisioning.JobWrite);
    AddPermissionPolicy(Permissions.Provisioning.JobExecute);
    AddPermissionPolicy(Permissions.Provisioning.TemplateManage);
    AddPermissionPolicy(Permissions.Provisioning.ServiceOrderRead);
    AddPermissionPolicy(Permissions.Provisioning.ServiceOrderWrite);

    AddPermissionPolicy(Permissions.Orders.OrderRead);
    AddPermissionPolicy(Permissions.Orders.OrderWrite);
    AddPermissionPolicy(Permissions.Orders.OrderApprove);
    AddPermissionPolicy(Permissions.Orders.OrderCancel);
    AddPermissionPolicy(Permissions.Orders.OrderFulfill);

    AddPermissionPolicy(Permissions.Customers.CustomerRead);
    AddPermissionPolicy(Permissions.Customers.CustomerWrite);
    AddPermissionPolicy(Permissions.Customers.CustomerKycVerify);
    AddPermissionPolicy(Permissions.Customers.CustomerSegmentManage);

    AddPermissionPolicy(Permissions.Crm.QuoteRead);
    AddPermissionPolicy(Permissions.Crm.QuoteWrite);
    AddPermissionPolicy(Permissions.Crm.QuoteApprove);
    AddPermissionPolicy(Permissions.Crm.AgreementRead);
    AddPermissionPolicy(Permissions.Crm.AgreementWrite);

    AddPermissionPolicy(Permissions.Subscriptions.SubscriptionRead);
    AddPermissionPolicy(Permissions.Subscriptions.SubscriptionWrite);
    AddPermissionPolicy(Permissions.Subscriptions.SubscriptionActivate);
    AddPermissionPolicy(Permissions.Subscriptions.SubscriptionSuspend);
    AddPermissionPolicy(Permissions.Subscriptions.SubscriptionCancel);
    AddPermissionPolicy(Permissions.Subscriptions.ProductRead);
    AddPermissionPolicy(Permissions.Subscriptions.ProductWrite);

    AddPermissionPolicy(Permissions.Catalog.ProductRead);
    AddPermissionPolicy(Permissions.Catalog.ProductWrite);
    AddPermissionPolicy(Permissions.Catalog.CategoryRead);
    AddPermissionPolicy(Permissions.Catalog.CategoryWrite);
    AddPermissionPolicy(Permissions.Catalog.OfferRead);
    AddPermissionPolicy(Permissions.Catalog.OfferWrite);
    AddPermissionPolicy(Permissions.Catalog.SpecificationRead);
    AddPermissionPolicy(Permissions.Catalog.SpecificationWrite);

    AddPermissionPolicy(Permissions.Audit.AuditRead);
    AddPermissionPolicy(Permissions.Audit.AuditWrite);
    AddPermissionPolicy(Permissions.Audit.AuditPurge);
    AddPermissionPolicy(Permissions.Audit.AuditAlertManage);
    AddPermissionPolicy(Permissions.Audit.AuditPolicyManage);

    AddPermissionPolicy(Permissions.Collections.CaseRead);
    AddPermissionPolicy(Permissions.Collections.CaseWrite);
    AddPermissionPolicy(Permissions.Collections.DunningManage);
    AddPermissionPolicy(Permissions.Collections.ArrangementManage);

    AddPermissionPolicy(Permissions.Network.ElementRead);
    AddPermissionPolicy(Permissions.Network.ElementWrite);
    AddPermissionPolicy(Permissions.Network.CapacityRead);
    AddPermissionPolicy(Permissions.Network.TopologyRead);

    AddPermissionPolicy(Permissions.Notifications.NotificationRead);
    AddPermissionPolicy(Permissions.Notifications.NotificationSend);
    AddPermissionPolicy(Permissions.Notifications.TemplateManage);
    AddPermissionPolicy(Permissions.Notifications.PreferenceManage);

    AddPermissionPolicy(Permissions.Telecom.ServiceRead);
    AddPermissionPolicy(Permissions.Telecom.ServiceWrite);
    AddPermissionPolicy(Permissions.Telecom.ServiceQualify);
    AddPermissionPolicy(Permissions.Telecom.ServiceActivate);
    AddPermissionPolicy(Permissions.Telecom.ServiceSuspend);
    AddPermissionPolicy(Permissions.Telecom.ServiceResume);
    AddPermissionPolicy(Permissions.Telecom.ServiceTerminate);
    AddPermissionPolicy(Permissions.Telecom.ServiceChange);
    AddPermissionPolicy(Permissions.Telecom.FtthRead);
    AddPermissionPolicy(Permissions.Telecom.FtthWrite);
    AddPermissionPolicy(Permissions.Telecom.AdslRead);
    AddPermissionPolicy(Permissions.Telecom.AdslWrite);
    AddPermissionPolicy(Permissions.Telecom.LteRead);
    AddPermissionPolicy(Permissions.Telecom.LteWrite);
    AddPermissionPolicy(Permissions.Telecom.TelephonyRead);
    AddPermissionPolicy(Permissions.Telecom.TelephonyWrite);
    AddPermissionPolicy(Permissions.Telecom.BundleRead);
    AddPermissionPolicy(Permissions.Telecom.BundleWrite);
    AddPermissionPolicy(Permissions.Telecom.AdapterManage);
    AddPermissionPolicy(Permissions.Telecom.AdapterRead);
    AddPermissionPolicy(Permissions.Telecom.UsageRead);
    AddPermissionPolicy(Permissions.Telecom.CdrIngest);
    AddPermissionPolicy(Permissions.Telecom.CdrMediate);
    AddPermissionPolicy(Permissions.Telecom.AlarmRead);
    AddPermissionPolicy(Permissions.Telecom.AlarmAck);
    AddPermissionPolicy(Permissions.Telecom.AlarmManage);
    AddPermissionPolicy(Permissions.Telecom.PerformanceRead);
    AddPermissionPolicy(Permissions.Telecom.SlaRead);
    AddPermissionPolicy(Permissions.Telecom.ReconciliationRead);
    AddPermissionPolicy(Permissions.Telecom.ReconciliationExecute);
    AddPermissionPolicy(Permissions.Telecom.MaintenanceManage);

    AddPermissionPolicy(Permissions.Ocs.BalanceRead);
    AddPermissionPolicy(Permissions.Ocs.BalanceWrite);
    AddPermissionPolicy(Permissions.Ocs.BalanceAdjust);
    AddPermissionPolicy(Permissions.Ocs.ReserveCredit);
    AddPermissionPolicy(Permissions.Ocs.ReservationDebit);
    AddPermissionPolicy(Permissions.Ocs.ReservationRelease);
    AddPermissionPolicy(Permissions.Ocs.CreditPoolRead);
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

builder.Services.AddSharedKernelServices(builder.Configuration);

var pgPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found. Set POSTGRES_CONNECTION_STRING environment variable or ConnectionStrings:DefaultConnection in configuration.");

if (!string.IsNullOrEmpty(pgPassword) && !connectionString.Contains("Password=", StringComparison.OrdinalIgnoreCase))
{
    connectionString = $"{connectionString};Password={pgPassword}";
}

string DbConn(string dbName)
{
    var connParts = connectionString.Split(';').Select(p =>
        p.StartsWith("Database=") ? $"Database=obss_{dbName}" : p);
    return string.Join(";", connParts);
}

void AddModuleDbContext<TDbContext>(string dbName)
    where TDbContext : EfDbContext
{
    builder.Services.AddDbContext<TDbContext>(o =>
        o.UseNpgsql(DbConn(dbName), npgsql => npgsql.MigrationsAssembly(typeof(TDbContext).Assembly.FullName)));
    builder.Services.AddScoped<EfDbContext>(sp => sp.GetRequiredService<TDbContext>());
}

AddModuleDbContext<Obss.IAM.Infrastructure.Persistence.IamDbContext>("iam");
AddModuleDbContext<Obss.CRM.Infrastructure.Persistence.CrmDbContext>("crm");
AddModuleDbContext<Obss.ProductCatalog.Infrastructure.Persistence.CatalogDbContext>("catalog");
AddModuleDbContext<Obss.Orders.Infrastructure.Persistence.OrderDbContext>("orders");
AddModuleDbContext<Obss.Subscriptions.Infrastructure.Persistence.SubscriptionDbContext>("subscriptions");
AddModuleDbContext<Obss.Rating.Infrastructure.Persistence.RatingDbContext>("rating");
AddModuleDbContext<Obss.Billing.Infrastructure.Persistence.BillingDbContext>("billing");
AddModuleDbContext<Obss.Invoices.Infrastructure.Persistence.InvoiceDbContext>("invoices");
AddModuleDbContext<Obss.Payments.Infrastructure.Persistence.PaymentDbContext>("payments");
AddModuleDbContext<Obss.Collections.Infrastructure.Persistence.CollectionDbContext>("collections");
AddModuleDbContext<Obss.ServiceInventory.Infrastructure.Persistence.ServiceDbContext>("service_inventory");
AddModuleDbContext<Obss.NetworkInventory.Infrastructure.Persistence.NetworkDbContext>("network_inventory");
AddModuleDbContext<Obss.NumberInventory.Infrastructure.Persistence.NumberDbContext>("number_inventory");
AddModuleDbContext<Obss.Provisioning.Infrastructure.Persistence.ProvisioningDbContext>("provisioning");
AddModuleDbContext<Obss.Workflow.Infrastructure.Persistence.WorkflowDbContext>("workflow");
AddModuleDbContext<Obss.Ticketing.Infrastructure.Persistence.TicketDbContext>("ticketing");
AddModuleDbContext<Obss.Notifications.Infrastructure.Persistence.NotificationDbContext>("notifications");
AddModuleDbContext<Obss.Reporting.Infrastructure.Persistence.ReportDbContext>("reporting");
AddModuleDbContext<Obss.Audit.Infrastructure.Persistence.AuditDbContext>("audit");
AddModuleDbContext<Obss.ApiGateway.Infrastructure.Persistence.GatewayDbContext>("gateway");
AddModuleDbContext<Obss.ServiceCatalog.Infrastructure.Persistence.ServiceCatalogDbContext>("service_catalog");
AddModuleDbContext<Obss.EventManagement.Infrastructure.Persistence.EventDbContext>("event_management");
AddModuleDbContext<Obss.ServiceQualification.Infrastructure.Persistence.ServiceQualificationDbContext>("service_qualification");
AddModuleDbContext<Obss.AAA.Infrastructure.Persistence.AaaDbContext>("aaa");
AddModuleDbContext<Obss.OCS.Infrastructure.Persistence.OcsDbContext>("ocs");

builder.Services.TryAddScoped(typeof(IRepository<>), typeof(EfRepository<>));

builder.Services.TryAddScoped<IUnitOfWork>(sp => new UnitOfWork(sp));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Obss.Audit.Application.Commands.CreateAuditEntry.CreateAuditEntryCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.IAM.Application.Commands.CreateUser.CreateUserCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.CRM.Application.Commands.CreateCustomer.CreateCustomerCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.ProductCatalog.Application.Commands.CreateProduct.CreateProductCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.Orders.Application.Commands.CreateProductOrder.CreateProductOrderCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.Subscriptions.Application.Commands.CreateSubscription.CreateSubscriptionCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.Rating.Application.Commands.CreateRatingRule.CreateRatingRuleCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.Billing.Application.Commands.GenerateBill.GenerateBillCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.Invoices.Application.Commands.CreateInvoiceFromBill.CreateInvoiceFromBillCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.Payments.Application.Commands.RecordPayment.RecordPaymentCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.Collections.Application.Commands.OpenCollectionCase.OpenCollectionCaseCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.Workflow.Application.Commands.CreateWorkflowDefinition.CreateWorkflowDefinitionCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.NetworkInventory.Application.Commands.CreateNetworkElement.CreateNetworkElementCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.Provisioning.Application.Commands.CreateProvisioningJob.CreateProvisioningJobCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.Notifications.Application.Commands.SendNotification.SendNotificationCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.Ticketing.Application.Commands.CreateTicket.CreateTicketCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.Reporting.Application.Commands.CreateReportDefinition.CreateReportDefinitionCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.ServiceInventory.Application.Commands.CreateService.CreateServiceCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.ApiGateway.Application.Commands.RegisterApiRoute.RegisterApiRouteCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.NumberInventory.Application.Commands.AddNumber.AddNumberCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.ServiceCatalog.Application.Commands.ServiceCategory.CreateServiceCategory.CreateServiceCategoryCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.EventManagement.Application.Commands.CreateSubscriptionCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.ServiceQualification.Application.Commands.CheckServiceQualification.CheckServiceQualificationCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.AAA.Application.Commands.RegisterNas.RegisterNasCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.OCS.Application.Commands.CreateBalance.CreateBalanceCommand).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
    // TransactionBehavior disabled - handlers call SaveChangesAsync explicitly via IUnitOfWork
});

builder.Services.AddValidatorsFromAssembly(typeof(Obss.Audit.Application.Commands.CreateAuditEntry.CreateAuditEntryCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.IAM.Application.Commands.CreateUser.CreateUserCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.CRM.Application.Commands.CreateCustomer.CreateCustomerCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.ProductCatalog.Application.Commands.CreateProduct.CreateProductCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.Orders.Application.Commands.CreateProductOrder.CreateProductOrderCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.Subscriptions.Application.Commands.CreateSubscription.CreateSubscriptionCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.Rating.Application.Commands.CreateRatingRule.CreateRatingRuleCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.Billing.Application.Commands.GenerateBill.GenerateBillCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.Invoices.Application.Commands.CreateInvoiceFromBill.CreateInvoiceFromBillCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.Payments.Application.Commands.RecordPayment.RecordPaymentCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.Collections.Application.Commands.OpenCollectionCase.OpenCollectionCaseCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.Workflow.Application.Commands.CreateWorkflowDefinition.CreateWorkflowDefinitionCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.NetworkInventory.Application.Commands.CreateNetworkElement.CreateNetworkElementCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.Provisioning.Application.Commands.CreateProvisioningJob.CreateProvisioningJobCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.Notifications.Application.Commands.SendNotification.SendNotificationCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.Ticketing.Application.Commands.CreateTicket.CreateTicketCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.Reporting.Application.Commands.CreateReportDefinition.CreateReportDefinitionCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.ServiceInventory.Application.Commands.CreateService.CreateServiceCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.ApiGateway.Application.Commands.RegisterApiRoute.RegisterApiRouteCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.NumberInventory.Application.Commands.AddNumber.AddNumberCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.ServiceCatalog.Application.Commands.ServiceCategory.CreateServiceCategory.CreateServiceCategoryCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.EventManagement.Application.Commands.CreateSubscriptionCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.ServiceQualification.Application.Commands.CheckServiceQualification.CheckServiceQualificationCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.AAA.Application.Commands.RegisterNas.RegisterNasCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.OCS.Application.Commands.CreateBalance.CreateBalanceCommandValidator).Assembly);

builder.Services.AddSingleton<RateLimitingConfiguration>();
builder.Services.AddSingleton<IModelCacheKeyFactory, Obss.SharedKernel.Infrastructure.Persistence.TenantModelCacheKeyFactory>();
builder.Services.AddOutboxProcessing(TimeSpan.FromSeconds(10));
builder.Services.AddRabbitMqConsumer();
builder.Services.AddAuditModule();
builder.Services.AddIamModule();
builder.Services.AddCrmModule();
builder.Services.AddCatalogModule();
builder.Services.AddOrderModule();
builder.Services.AddSubscriptionModule();
builder.Services.AddRatingModule();
builder.Services.AddBillingModule();
builder.Services.AddInvoiceModule();
builder.Services.AddPaymentModule();
builder.Services.AddCollectionModule();
builder.Services.AddNetworkModule();
builder.Services.AddServiceInventoryModule();
builder.Services.AddWorkflowModule();
builder.Services.AddProvisioningModule();
builder.Services.AddNotificationsModule(builder.Configuration);
builder.Services.AddTicketingModule();
builder.Services.AddReportingModule();
builder.Services.AddApiGatewayModule();
builder.Services.AddNumberInventoryModule();
builder.Services.AddServiceCatalogModule();
builder.Services.AddEventModule();
builder.Services.AddServiceQualificationModule();
builder.Services.AddAaaModule();
builder.Services.AddOcsModule();

var pgConnString = connectionString;
var healthChecksBuilder = builder.Services.AddHealthChecks();
healthChecksBuilder
    .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379",
        name: "redis", tags: ["ready", "health"])
    .AddRabbitMQ(async (_) =>
    {
        var config = builder.Configuration.GetSection("RabbitMq");
        var factory = new RabbitMQ.Client.ConnectionFactory
        {
            HostName = config["Host"] ?? "localhost",
            Port = int.TryParse(config["Port"], out var port) ? port : 5672,
            UserName = config["Username"] ?? "guest",
            Password = config["Password"] ?? "guest",
            VirtualHost = config["VirtualHost"] ?? "/",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };
        return await factory.CreateConnectionAsync();
    }, "rabbitmq", tags: ["ready", "health"]);

void AddDbHealthCheck<TDbContext>(string name) where TDbContext : DbContext
    => healthChecksBuilder.AddDbContextCheck<TDbContext>(
        name: $"db_{name}",
        tags: ["database", "ready", "health"]);

AddDbHealthCheck<Obss.IAM.Infrastructure.Persistence.IamDbContext>("iam");
AddDbHealthCheck<Obss.CRM.Infrastructure.Persistence.CrmDbContext>("crm");
AddDbHealthCheck<Obss.ProductCatalog.Infrastructure.Persistence.CatalogDbContext>("catalog");
AddDbHealthCheck<Obss.Orders.Infrastructure.Persistence.OrderDbContext>("orders");
AddDbHealthCheck<Obss.Subscriptions.Infrastructure.Persistence.SubscriptionDbContext>("subscriptions");
AddDbHealthCheck<Obss.Rating.Infrastructure.Persistence.RatingDbContext>("rating");
AddDbHealthCheck<Obss.Billing.Infrastructure.Persistence.BillingDbContext>("billing");
AddDbHealthCheck<Obss.Invoices.Infrastructure.Persistence.InvoiceDbContext>("invoices");
AddDbHealthCheck<Obss.Payments.Infrastructure.Persistence.PaymentDbContext>("payments");
AddDbHealthCheck<Obss.Collections.Infrastructure.Persistence.CollectionDbContext>("collections");
AddDbHealthCheck<Obss.ServiceInventory.Infrastructure.Persistence.ServiceDbContext>("service_inventory");
AddDbHealthCheck<Obss.NetworkInventory.Infrastructure.Persistence.NetworkDbContext>("network_inventory");
AddDbHealthCheck<Obss.NumberInventory.Infrastructure.Persistence.NumberDbContext>("number_inventory");
AddDbHealthCheck<Obss.Provisioning.Infrastructure.Persistence.ProvisioningDbContext>("provisioning");
AddDbHealthCheck<Obss.Workflow.Infrastructure.Persistence.WorkflowDbContext>("workflow");
AddDbHealthCheck<Obss.Ticketing.Infrastructure.Persistence.TicketDbContext>("ticketing");
AddDbHealthCheck<Obss.Notifications.Infrastructure.Persistence.NotificationDbContext>("notifications");
AddDbHealthCheck<Obss.Reporting.Infrastructure.Persistence.ReportDbContext>("reporting");
AddDbHealthCheck<Obss.Audit.Infrastructure.Persistence.AuditDbContext>("audit");
AddDbHealthCheck<Obss.ApiGateway.Infrastructure.Persistence.GatewayDbContext>("apigateway");
AddDbHealthCheck<Obss.ServiceCatalog.Infrastructure.Persistence.ServiceCatalogDbContext>("service_catalog");
AddDbHealthCheck<Obss.EventManagement.Infrastructure.Persistence.EventDbContext>("events");
AddDbHealthCheck<Obss.ServiceQualification.Infrastructure.Persistence.ServiceQualificationDbContext>("service_qualification");
AddDbHealthCheck<Obss.AAA.Infrastructure.Persistence.AaaDbContext>("aaa");
AddDbHealthCheck<Obss.OCS.Infrastructure.Persistence.OcsDbContext>("ocs");

var infraAsms = new[] {
    typeof(Obss.IAM.Infrastructure.Persistence.IamDbContext).Assembly,
    typeof(Obss.CRM.Infrastructure.Persistence.CrmDbContext).Assembly,
    typeof(Obss.ProductCatalog.Infrastructure.Persistence.CatalogDbContext).Assembly,
    typeof(Obss.Orders.Infrastructure.Persistence.OrderDbContext).Assembly,
    typeof(Obss.Subscriptions.Infrastructure.Persistence.SubscriptionDbContext).Assembly,
    typeof(Obss.Rating.Infrastructure.Persistence.RatingDbContext).Assembly,
    typeof(Obss.Billing.Infrastructure.Persistence.BillingDbContext).Assembly,
    typeof(Obss.Invoices.Infrastructure.Persistence.InvoiceDbContext).Assembly,
    typeof(Obss.Payments.Infrastructure.Persistence.PaymentDbContext).Assembly,
    typeof(Obss.Collections.Infrastructure.Persistence.CollectionDbContext).Assembly,
    typeof(Obss.ServiceInventory.Infrastructure.Persistence.ServiceDbContext).Assembly,
    typeof(Obss.NetworkInventory.Infrastructure.Persistence.NetworkDbContext).Assembly,
    typeof(Obss.Provisioning.Infrastructure.Persistence.ProvisioningDbContext).Assembly,
    typeof(Obss.Workflow.Infrastructure.Persistence.WorkflowDbContext).Assembly,
    typeof(Obss.Ticketing.Infrastructure.Persistence.TicketDbContext).Assembly,
    typeof(Obss.Notifications.Infrastructure.Persistence.NotificationDbContext).Assembly,
    typeof(Obss.Reporting.Infrastructure.Persistence.ReportDbContext).Assembly,
    typeof(Obss.Audit.Infrastructure.Persistence.AuditDbContext).Assembly,
    typeof(Obss.ApiGateway.Infrastructure.Persistence.GatewayDbContext).Assembly,
    typeof(Obss.NumberInventory.Infrastructure.Persistence.NumberDbContext).Assembly,
    typeof(Obss.ServiceCatalog.Infrastructure.Persistence.ServiceCatalogDbContext).Assembly,
    typeof(Obss.EventManagement.Infrastructure.Persistence.EventDbContext).Assembly,
    typeof(Obss.ServiceQualification.Infrastructure.Persistence.ServiceQualificationDbContext).Assembly,
    typeof(Obss.AAA.Infrastructure.Persistence.AaaDbContext).Assembly,
    typeof(Obss.OCS.Infrastructure.Persistence.OcsDbContext).Assembly,
};
var skipInterfaces = new HashSet<string> { "IDisposable", "IAsyncDisposable", "IEquatable`1" };
foreach (var asm in infraAsms)
{
    foreach (var type in asm.GetTypes().Where(t => t.IsClass && !t.IsAbstract))
    {
        foreach (var iface in type.GetInterfaces())
        {
            if (skipInterfaces.Contains(iface.Name)) continue;
            if (iface.Namespace?.StartsWith("Obss") != true) continue;
            if (!builder.Services.Any(s => s.ServiceType == iface))
                builder.Services.AddScoped(iface, type);
        }
    }
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    // Auto-migration disabled - run manually via: dotnet ef database update
    // Tables already exist from previous migration runs
}

app.UseCors("AllowFrontend");
app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationMiddleware>();
app.UseMiddleware<Obss.Host.Middleware.ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseMiddleware<ApiKeyAuthMiddleware>();
app.UseAuthorization();
app.UseMiddleware<TenantValidationMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

app.MapAuditEndpoints();
app.MapIamEndpoints();
app.MapCrmEndpoints();
app.MapCatalogEndpoints();
app.MapProductOrderEndpoints();
app.MapOrderRedirects();
app.MapSubscriptionEndpoints();
app.MapRatingEndpoints();
app.MapBillingEndpoints();
app.MapInvoiceEndpoints();
app.MapPaymentEndpoints();
app.MapCollectionEndpoints();
app.MapWorkflowEndpoints();
app.MapProvisioningEndpoints();
app.MapServiceInventoryEndpoints();
app.MapNetworkEndpoints();
app.MapNotificationsEndpoints();
app.MapTicketingEndpoints();
app.MapReportingEndpoints();
app.MapApiGatewayEndpoints();
app.MapNumberInventoryEndpoints();
app.MapServiceCatalogEndpoints();
app.MapEventEndpoints();
app.MapServiceQualificationEndpoints();
app.MapAaaEndpoints();
app.MapOcsEndpoints();

var healthOptions = new HealthCheckOptions
{
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
    }
};

app.MapHealthChecks("/health", healthOptions).AllowAnonymous();
app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = _ => false,
    ResultStatusCodes = { [HealthStatus.Healthy] = StatusCodes.Status200OK }
}).AllowAnonymous();
app.MapHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
    }
}).AllowAnonymous();
app.MapGet("/health/detailed", async (HealthCheckService healthCheckService) =>
{
    var report = await healthCheckService.CheckHealthAsync();
    return Results.Ok(new
    {
        Status = report.Status.ToString(),
        Timestamp = DateTime.UtcNow,
        Entries = report.Entries.ToDictionary(
            e => e.Key,
            e => new { e.Value.Status, e.Value.Description, e.Value.Duration }
        )
    });
}).WithTags("Health").AllowAnonymous();

app.MapPost("/api/v1/bootstrap/tenant", async (
    BootstrapTenantRequest request,
    IMediator mediator,
    Obss.IAM.Infrastructure.Persistence.IamDbContext dbContext) =>
{
    var tenantExists = await dbContext.Tenants.AnyAsync();
    if (tenantExists)
        return Results.Conflict(new { Error = "Tenant already exists. Bootstrap can only run once." });

    var command = new Obss.IAM.Application.Commands.CreateTenant.CreateTenantCommand(
        request.Name,
        request.Slug,
        null,
        null,
        request.AdminUsername,
        request.AdminEmail,
        request.AdminFirstName,
        request.AdminLastName);

    var result = await mediator.Send(command);
    return result.IsSuccess
        ? Results.Created($"/api/v1/iam/tenants/{result.Value.Id}", result.Value)
        : Results.Conflict(result.Error);
}).AllowAnonymous();

await app.RunAsync();

internal sealed record BootstrapTenantRequest(
    string Name,
    string Slug,
    string AdminUsername,
    string AdminEmail,
    string AdminFirstName,
    string AdminLastName);
