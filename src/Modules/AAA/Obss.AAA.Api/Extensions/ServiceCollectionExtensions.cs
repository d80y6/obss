using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.Mappings;
using Obss.AAA.Domain.Events;
using Obss.AAA.Infrastructure.EventHandlers;
using Obss.AAA.Infrastructure.Persistence.Repositories;

namespace Obss.AAA.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAaaModule(this IServiceCollection services)
    {
        services.AddScoped<INasRepository, NasRepository>();
        services.AddScoped<IRadiusSessionRepository, RadiusSessionRepository>();
        services.AddScoped<IAaaAuditLogRepository, AaaAuditLogRepository>();

        services.AddScoped<INotificationHandler<RadiusSessionStartedDomainEvent>, LogSessionStartedHandler>();
        services.AddScoped<INotificationHandler<RadiusSessionStoppedDomainEvent>, LogSessionStoppedHandler>();

        AaaMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapAaaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/aaa")
            .WithTags("AAA");

        NasEndpoints.Map(group);
        SessionEndpoints.Map(group);
        MetricsEndpoints.Map(group);
        AuditLogEndpoints.Map(group);

        return app;
    }
}
