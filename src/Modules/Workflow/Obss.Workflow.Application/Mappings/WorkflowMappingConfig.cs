using Mapster;
using Obss.Workflow.Application.DTOs;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Application.Mappings;

public static class WorkflowMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<WorkflowDefinition, WorkflowDefinitionDto>.NewConfig()
            .Map(dest => dest.Category, src => src.Category.ToString())
            .Map(dest => dest.Steps, src => src.Steps.Adapt<List<WorkflowStepDto>>())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<WorkflowStep, WorkflowStepDto>.NewConfig()
            .Map(dest => dest.StepType, src => src.StepType.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<WorkflowInstance, WorkflowInstanceDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Tasks, src => src.Tasks.Adapt<List<WorkflowTaskDto>>())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<WorkflowTaskInstance, WorkflowTaskDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<WorkflowSla, WorkflowSlaDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<WorkflowMetric, WorkflowMetricDto>.NewConfig()
            .Map(dest => dest.MetricType, src => src.MetricType.ToString())
            .Map(dest => dest.Id, src => src.Id);
    }
}
