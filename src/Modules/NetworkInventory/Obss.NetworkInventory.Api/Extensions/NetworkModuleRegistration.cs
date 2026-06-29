using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.NetworkInventory.Api.Endpoints;
using Obss.NetworkInventory.Application.Mappings;
using Obss.NetworkInventory.Infrastructure.Persistence;

namespace Obss.NetworkInventory.Api.Extensions;

public static class NetworkModuleRegistration
{
    public static IServiceCollection AddNetworkModule(this IServiceCollection services)
    {
        NetworkMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapNetworkEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/network")
            .WithTags("Network Inventory");

        NetworkElementEndpoints.Map(group);
        SubnetEndpoints.Map(group);
        VLANEndpoints.Map(group);
        OLTEndpoints.Map(group);
        TopologyEndpoints.Map(group);
        LinkEndpoints.Map(group);
        CapacityEndpoints.Map(group);

        return app;
    }
}
