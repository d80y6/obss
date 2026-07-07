using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.Audit.Api.Endpoints;
using Obss.Audit.Application.Abstractions;
using Obss.Audit.Application.Mappings;
using Obss.Audit.Infrastructure.BackgroundJobs;
using Obss.Audit.Infrastructure.Persistence;
using Obss.Audit.Infrastructure.Persistence.Repositories;
using Obss.Audit.Infrastructure.Services;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Audit.Api.Extensions;

public static class AuditModuleRegistration
{
    public static IServiceCollection AddAuditModule(this IServiceCollection services)
    {
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuditAlertRepository, AuditAlertRepository>();
        services.AddHostedService<AuditRetentionJob>();
        services.AddHostedService<AuditAlertDetectionJob>();
        AuditMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/audit")
            .WithTags("Audit");

        AuditEndpoints.Map(group);

        return app;
    }
}
