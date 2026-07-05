using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.ServiceInventory.Api.Endpoints;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Application.Mappings;
using Obss.ServiceInventory.Infrastructure.Persistence;
using Obss.ServiceInventory.Infrastructure.Persistence.Repositories;

namespace Obss.ServiceInventory.Api.Extensions;

public static class ServiceModuleRegistration
{
    public static IServiceCollection AddServiceInventoryModule(this IServiceCollection services)
    {
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IServiceTopologyRepository, ServiceTopologyRepository>();
        services.AddScoped<IResourceDiscoveryJobRepository, ResourceDiscoveryJobRepository>();
        ServiceMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapServiceInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/service-inventory")
            .WithTags("ServiceInventory");

        ServiceEndpoints.Map(group);

        return app;
    }
}
