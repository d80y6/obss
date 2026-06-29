using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.Reporting.Api.Endpoints;
using Obss.Reporting.Application.Abstractions;
using Obss.Reporting.Application.BackgroundJobs;
using Obss.Reporting.Application.Mappings;
using Obss.Reporting.Infrastructure.Persistence;
using Obss.Reporting.Infrastructure.Persistence.Repositories;
using Obss.Reporting.Infrastructure.Services;

namespace Obss.Reporting.Api.Extensions;

public static class ReportingModuleRegistration
{
    public static IServiceCollection AddReportingModule(this IServiceCollection services)
    {
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IReportGenerator, ReportGenerator>();
        services.AddHostedService<ScheduledReportJob>();

        ReportingMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapReportingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/reporting")
            .WithTags("Reporting");

        ReportEndpoints.Map(group);
        DashboardEndpoints.Map(group);

        return app;
    }
}
