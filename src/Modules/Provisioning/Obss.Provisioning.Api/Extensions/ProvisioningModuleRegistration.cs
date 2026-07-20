using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.Provisioning.Api.Endpoints;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.BackgroundJobs;
using Obss.Provisioning.Application.Diagnostics;
using Obss.Provisioning.Application.Mappings;
using Obss.Provisioning.Infrastructure.Persistence;
using Obss.Provisioning.Infrastructure.Persistence.Repositories;
using Obss.Provisioning.Infrastructure.Adapters.Common;
using Obss.Provisioning.Infrastructure.Services;

namespace Obss.Provisioning.Api.Extensions;

public static class ProvisioningModuleRegistration
{
    public static IServiceCollection AddProvisioningModule(this IServiceCollection services)
    {
        services.AddScoped<IProvisioningJobRepository, ProvisioningJobRepository>();
        services.AddScoped<IProvisioningTemplateRepository, ProvisioningTemplateRepository>();
        services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();

        services.AddSingleton<ProvisioningMetrics>();
        services.AddSingleton<IAdapterRegistry, AdapterRegistry>();
        services.AddScoped<IProvisioningAdapter, NetworkProvisioningAdapter>();
        services.AddScoped<IProvisioningAdapter, DnsSetupAdapter>();
        services.AddScoped<IProvisioningAdapter, AccountSetupAdapter>();
        services.AddScoped<IProvisioningAdapter, TestProvisioningAdapter>();

        services.AddHostedService<ProvisioningJobProcessor>();

        ProvisioningMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapProvisioningEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/provisioning")
            .WithTags("Provisioning");

        ProvisioningEndpoints.Map(group);

        return app;
    }
}
