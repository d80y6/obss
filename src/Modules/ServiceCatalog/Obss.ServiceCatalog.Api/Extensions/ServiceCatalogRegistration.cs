using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.ServiceCatalog.Api.Endpoints;
using Obss.ServiceCatalog.Application.Mappings;
using Obss.ServiceCatalog.Infrastructure;

namespace Obss.ServiceCatalog.Api.Extensions;

public static class ServiceCatalogRegistration
{
    public static IServiceCollection AddServiceCatalogModule(this IServiceCollection services)
    {
        services.AddServiceCatalogInfrastructure();
        ServiceCatalogMappingConfig.Configure();
        return services;
    }

    public static IEndpointRouteBuilder MapServiceCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/service-catalog").WithTags("Service Catalog");
        ServiceCategoryEndpoints.Map(group);
        ServiceCandidateEndpoints.Map(group);
        ServiceSpecificationEndpoints.Map(group);
        return app;
    }
}
