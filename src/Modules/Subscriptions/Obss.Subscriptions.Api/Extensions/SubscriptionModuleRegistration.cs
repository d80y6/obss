using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.Subscriptions.Api.Endpoints;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.BackgroundJobs;
using Obss.Subscriptions.Application.Mappings;
using Obss.Subscriptions.Application.Services;
using Obss.Subscriptions.Domain.Services;
using Obss.Subscriptions.Infrastructure.Persistence;
using Obss.Subscriptions.Infrastructure.Persistence.Repositories;

namespace Obss.Subscriptions.Api.Extensions;

public static class SubscriptionModuleRegistration
{
    public static IServiceCollection AddSubscriptionModule(this IServiceCollection services)
    {
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ISubscriptionEntitlementRepository, SubscriptionEntitlementRepository>();

        services.AddScoped<IEntitlementValidator, EntitlementValidator>();

        services.AddHostedService<SubscriptionRenewalJob>();

        SubscriptionMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapSubscriptionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/subscriptions")
            .WithTags("Subscriptions");

        SubscriptionEndpoints.Map(group);

        return app;
    }
}
