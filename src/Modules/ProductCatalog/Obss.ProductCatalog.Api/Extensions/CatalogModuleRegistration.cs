using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.ProductCatalog.Api.Endpoints;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.Mappings;
using Obss.ProductCatalog.Infrastructure.Persistence;
using Obss.ProductCatalog.Infrastructure.Persistence.Repositories;
using Obss.ProductCatalog.Infrastructure.Services;

namespace Obss.ProductCatalog.Api.Extensions;

public static class CatalogModuleRegistration
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services)
    {
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOfferRepository, OfferRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductConfigurationRepository, ProductConfigurationRepository>();
        services.AddScoped<IProductConfigurationValidator, ProductConfigurationValidator>();
        services.AddScoped<IProductSpecificationRepository, ProductSpecificationRepository>();

        CatalogMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/catalog")
            .WithTags("Catalog");

        CatalogEndpoints.Map(group);
        ProductEndpoints.Map(group);
        OfferEndpoints.Map(group);
        CategoryEndpoints.Map(group);
        ProductSpecificationEndpoints.Map(group);

        return app;
    }
}
