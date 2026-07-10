using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.Orders.Api.Endpoints;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.BackgroundJobs;
using Obss.Orders.Application.Mappings;
using Obss.Orders.Application.Services;
using Obss.Orders.Infrastructure.Persistence;
using Obss.Orders.Infrastructure.Persistence.Repositories;

namespace Obss.Orders.Api.Extensions;

public static class ProductOrderModuleRegistration
{
    public static IServiceCollection AddOrderModule(this IServiceCollection services)
    {
        services.AddScoped<IProductOrderRepository, ProductOrderRepository>();
        services.AddScoped<IOrderFulfillmentRepository, OrderFulfillmentRepository>();

        services.AddScoped<OrderValidationService>();

        services.AddHostedService<OrderFulfillmentJob>();

        ProductOrderMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapProductOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/productOrder")
            .WithTags("Product Orders");

        ProductOrderEndpoints.Map(group);

        return app;
    }

    // Redirect old /orders paths to new /productOrder paths
    public static IEndpointRouteBuilder MapOrderRedirects(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v{version:apiVersion}/orders/{**rest}", (string? rest) =>
            Results.Redirect($"/api/v1/productOrder/{rest ?? ""}", permanent: true))
            .ExcludeFromDescription();

        app.MapPost("/api/v{version:apiVersion}/orders/{**rest}", (string? rest) =>
            Results.Redirect($"/api/v1/productOrder/{rest ?? ""}", permanent: true))
            .ExcludeFromDescription();

        app.MapPatch("/api/v{version:apiVersion}/orders/{**rest}", (string? rest) =>
            Results.Redirect($"/api/v1/productOrder/{rest ?? ""}", permanent: true))
            .ExcludeFromDescription();

        app.MapDelete("/api/v{version:apiVersion}/orders/{**rest}", (string? rest) =>
            Results.Redirect($"/api/v1/productOrder/{rest ?? ""}", permanent: true))
            .ExcludeFromDescription();

        return app;
    }
}
