using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.NumberInventory.Api.Endpoints;
using Obss.NumberInventory.Application.Abstractions;
using Obss.NumberInventory.Application.Mappings;
using Obss.NumberInventory.Infrastructure.Persistence.Repositories;

namespace Obss.NumberInventory.Api.Extensions;

public static class NumberInventoryModuleRegistration
{
    public static IServiceCollection AddNumberInventoryModule(this IServiceCollection services)
    {
        services.AddScoped<ITelephoneNumberRepository, TelephoneNumberRepository>();

        NumberInventoryMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapNumberInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/number-inventory")
            .WithTags("NumberInventory");

        NumberEndpoints.Map(group);

        return app;
    }
}
