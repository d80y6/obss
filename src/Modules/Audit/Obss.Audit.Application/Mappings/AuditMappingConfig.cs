using Mapster;
using Obss.Audit.Application.DTOs;
using Obss.Audit.Domain.Entities;

namespace Obss.Audit.Application.Mappings;

public static class AuditMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<AuditEntry, AuditEntryDto>.NewConfig()
            .Map(dest => dest.Action, src => src.Action.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<AuditPolicy, AuditPolicyDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<AuditAlert, AuditAlertDto>.NewConfig()
            .Map(dest => dest.Severity, src => src.Severity.ToString())
            .Map(dest => dest.AlertType, src => src.AlertType.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<AuditAlertRule, AuditAlertRuleDto>.NewConfig()
            .Map(dest => dest.AlertType, src => src.AlertType.ToString())
            .Map(dest => dest.Severity, src => src.Severity.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<AuditEntry, ComplianceReportDto>.NewConfig();
        TypeAdapterConfig<AuditEntry, ComplianceSummaryDto>.NewConfig();
    }
}
