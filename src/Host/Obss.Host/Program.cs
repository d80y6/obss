using System.Reflection;
using System.Text.Json.Serialization;
using Asp.Versioning;
using EFCore.NamingConventions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Behaviors;
using Obss.SharedKernel.Infrastructure;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.Subscriptions.Api.Extensions;
using Obss.Ticketing.Api.Extensions;
using Obss.Workflow.Api.Extensions;
using Serilog;

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
                .Where(p => p.Value.Nullable)
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

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
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

builder.Services.TryAddScoped(typeof(IRepository<>), typeof(EfRepository<>));

builder.Services.TryAddScoped<IUnitOfWork>(sp => new UnitOfWork(sp));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Obss.Audit.Application.Commands.CreateAuditEntry.CreateAuditEntryCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.IAM.Application.Commands.CreateUser.CreateUserCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.CRM.Application.Commands.CreateCustomer.CreateCustomerCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.ProductCatalog.Application.Commands.CreateProduct.CreateProductCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Obss.Orders.Application.Commands.CreateOrder.CreateOrderCommand).Assembly);
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
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    // TransactionBehavior disabled - handlers call SaveChangesAsync explicitly via IUnitOfWork
});

builder.Services.AddValidatorsFromAssembly(typeof(Obss.Audit.Application.Commands.CreateAuditEntry.CreateAuditEntryCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.IAM.Application.Commands.CreateUser.CreateUserCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.CRM.Application.Commands.CreateCustomer.CreateCustomerCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.ProductCatalog.Application.Commands.CreateProduct.CreateProductCommandValidator).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Obss.Orders.Application.Commands.CreateOrder.CreateOrderCommandValidator).Assembly);
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
builder.Services.AddNotificationsModule();
builder.Services.AddTicketingModule();
builder.Services.AddReportingModule();
builder.Services.AddApiGatewayModule();
builder.Services.AddNumberInventoryModule();
builder.Services.AddServiceCatalogModule();
builder.Services.AddEventModule();

var pgConnString = connectionString;
builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379")
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
    }, "rabbitmq");

builder.Services.AddHealthChecks()
    .AddDbContextCheck<Obss.IAM.Infrastructure.Persistence.IamDbContext>("postgresql");

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
app.UseMiddleware<Obss.Host.Middleware.ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseMiddleware<ApiKeyAuthMiddleware>();
app.UseAuthorization();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

app.MapAuditEndpoints();
app.MapIamEndpoints();
app.MapCrmEndpoints();
app.MapCatalogEndpoints();
app.MapOrderEndpoints();
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

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResultStatusCodes =
    {
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy] = StatusCodes.Status200OK,
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
    }
}).AllowAnonymous();
app.MapGet("/health/detailed", async (Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService healthCheckService) =>
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

await app.RunAsync();
