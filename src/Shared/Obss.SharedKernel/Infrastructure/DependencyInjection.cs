using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Diagnostics;
using Obss.SharedKernel.Infrastructure.EventBus;
using Obss.SharedKernel.Infrastructure.Services;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Obss.SharedKernel.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedKernelServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis")
                                    ?? configuration["ConnectionStrings:Redis"]
                                    ?? "localhost:6379";

        var otelEndpoint = configuration["OpenTelemetry:Endpoint"]
                           ?? "http://localhost:4317";

        var serviceName = configuration["OpenTelemetry:ServiceName"]
                          ?? "Obss.SharedKernel";

        services.Configure<RabbitMqConfiguration>(
            configuration.GetSection(RabbitMqConfiguration.SectionName));

        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddMemoryCache();

        services.TryAddScoped<ICurrentUser, CurrentUserService>();
        services.TryAddScoped<ICurrentTenant, CurrentTenantService>();
        services.TryAddScoped<ITenantStore, DefaultTenantStore>();
        services.TryAddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.TryAddSingleton<IDistributedCacheService, CacheService>();
        services.TryAddSingleton<IEventBus, EventBusService>();
        services.TryAddScoped<IOutboxService, OutboxService>();
        services.TryAddScoped<IInboxService, InboxService>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = configuration.GetValue<string>("Redis:InstanceName") ?? "Obss";
        });

        services.TryAddSingleton<OutboxMetrics>();
        services.TryAddSingleton<RabbitMqMetrics>();

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddRedisInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(otelEndpoint);
                })
                .SetSampler(new AlwaysOnSampler()))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(otelEndpoint);
                }));

        return services;
    }
}
