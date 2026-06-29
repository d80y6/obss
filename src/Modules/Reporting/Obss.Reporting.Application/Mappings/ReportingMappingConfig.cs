using Mapster;
using Obss.Reporting.Application.DTOs;
using Obss.Reporting.Domain.Entities;

namespace Obss.Reporting.Application.Mappings;

public static class ReportingMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<ReportDefinition, ReportDefinitionDto>.NewConfig()
            .Map(dest => dest.ReportType, src => src.ReportType.ToString())
            .Map(dest => dest.OutputFormat, src => src.OutputFormat.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<ReportExecution, ReportExecutionDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<DashboardWidget, DashboardWidgetDto>.NewConfig()
            .Map(dest => dest.WidgetType, src => src.WidgetType.ToString())
            .Map(dest => dest.Size, src => src.Size.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<ScheduledReport, ScheduledReportDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);
    }
}
