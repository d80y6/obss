using Microsoft.AspNetCore.Builder;
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

public static class OrderModuleRegistration
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

    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/orders")
            .WithTags("Orders");

        OrderEndpoints.Map(group);

        return app;
    }
}
