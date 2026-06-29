using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.Collections.Api.Endpoints;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Application.BackgroundJobs;
using Obss.Collections.Application.Mappings;
using Obss.Collections.Infrastructure.Persistence;
using Obss.Collections.Infrastructure.Persistence.Repositories;

namespace Obss.Collections.Api.Extensions;

public static class CollectionModuleRegistration
{
    public static IServiceCollection AddCollectionModule(this IServiceCollection services)
    {
        services.AddScoped<ICollectionCaseRepository, CollectionCaseRepository>();
        services.AddScoped<IDunningPolicyRepository, DunningPolicyRepository>();

        services.AddHostedService<DunningProcessingJob>();

        CollectionMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapCollectionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/collections")
            .WithTags("Collections");

        CollectionEndpoints.Map(group);

        return app;
    }
}
