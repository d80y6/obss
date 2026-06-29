using Mapster;
using Obss.Provisioning.Application.DTOs;
using Obss.Provisioning.Domain.Entities;

namespace Obss.Provisioning.Application.Mappings;

public static class ProvisioningMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<ProvisioningJob, ProvisioningJobDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Action, src => src.Action.ToString())
            .Map(dest => dest.Tasks, src => src.Tasks.Adapt<List<ProvisioningTaskDto>>())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<ProvisioningTask, ProvisioningTaskDto>.NewConfig()
            .Map(dest => dest.TaskType, src => src.TaskType.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<ProvisioningTemplate, ProvisioningTemplateDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);
    }
}
