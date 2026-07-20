using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.EventManagement.Api.Endpoints;
using Obss.EventManagement.Application.Abstractions;
using Obss.EventManagement.Infrastructure.Persistence.Repositories;
using Obss.EventManagement.Infrastructure.Diagnostics;
using Obss.EventManagement.Infrastructure.Services;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.EventManagement.Api.Extensions;

public static class EventModuleRegistration
{
    public static IServiceCollection AddEventModule(this IServiceCollection services)
    {
        services.AddScoped<IEventSubscriptionRepository, EventSubscriptionRepository>();
        services.AddScoped<IWebhookEventRepository, WebhookEventRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        services.AddSingleton<WebhookMetrics>();
        services.AddHttpClient<WebhookDispatcher>();
        services.AddHostedService<WebhookDispatcher>();

        return services;
    }

    public static IEndpointRouteBuilder MapEventEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/events")
            .WithTags("Events");

        EventManagementEndpoints.Map(group);

        return app;
    }
}
