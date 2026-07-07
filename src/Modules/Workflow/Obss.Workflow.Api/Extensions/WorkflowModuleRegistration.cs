using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Obss.Workflow.Api.Endpoints;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Application.BackgroundJobs;
using Obss.Workflow.Application.Mappings;
using Obss.Workflow.Domain.Services;
using Obss.Workflow.Infrastructure.Persistence;
using Obss.Workflow.Infrastructure.Persistence.Repositories;
using Obss.Workflow.Infrastructure.Services;

namespace Obss.Workflow.Api.Extensions;

public static class WorkflowModuleRegistration
{
    public static IServiceCollection AddWorkflowModule(this IServiceCollection services)
    {
        services.AddScoped<IWorkflowDefinitionRepository, WorkflowDefinitionRepository>();
        services.AddScoped<IWorkflowInstanceRepository, WorkflowInstanceRepository>();
        services.AddScoped<IWorkflowSlaRepository, WorkflowSlaRepository>();
        services.AddScoped<IWorkflowEngine, WorkflowEngine>();

        services.AddHostedService<WorkflowTaskExecutionJob>();
        services.AddHostedService<WorkflowSlaMonitorJob>();

        WorkflowMappingConfig.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapWorkflowEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/workflow")
            .WithTags("Workflow");

        WorkflowDefinitionEndpoints.Map(group);
        WorkflowInstanceEndpoints.Map(group);
        WorkflowSlaEndpoints.Map(group);
        WorkflowMonitoringEndpoints.Map(group);

        return app;
    }
}
