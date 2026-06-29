using MediatR;
using Obss.Audit.Application.Abstractions;
using Obss.Audit.Application.DTOs;
using Obss.Audit.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Commands.GenerateComplianceReport;

public sealed class GenerateComplianceReportCommandHandler : IRequestHandler<GenerateComplianceReportCommand, Result<ComplianceReportDto>>
{
    private readonly IAuditEntryRepository _entryRepository;
    private readonly IRepository<AuditPolicy> _policyRepository;
    private readonly ICurrentTenant _currentTenant;

    public GenerateComplianceReportCommandHandler(
        IAuditEntryRepository entryRepository,
        IRepository<AuditPolicy> policyRepository,
        ICurrentTenant currentTenant)
    {
        _entryRepository = entryRepository;
        _policyRepository = policyRepository;
        _currentTenant = currentTenant;
    }

    public async Task<Result<ComplianceReportDto>> Handle(GenerateComplianceReportCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId;

        var byAction = await _entryRepository.GetCountByActionAsync(tenantId, cancellationToken);
        var byEntity = await _entryRepository.GetCountByEntityTypeAsync(tenantId, cancellationToken);
        var stats = await _entryRepository.GetComplianceSummaryAsync(cancellationToken);

        var policies = await _policyRepository.GetAllAsync(cancellationToken);
        var activePolicies = policies.Where(p => p.IsActive).ToList();

        var retentionBreaches = 0;
        foreach (var policy in activePolicies)
        {
            var cutoff = DateTime.UtcNow.AddDays(-policy.RetentionDays);
            var breaches = await _entryRepository.CountEntriesOlderThanAsync(policy.EntityType, cutoff, cancellationToken);
            retentionBreaches += breaches;
        }

        var compliancePercent = stats.TotalEntries > 0
            ? Math.Round((double)(stats.TotalEntries - retentionBreaches) / stats.TotalEntries * 100, 2)
            : 100;

        return Result.Success(new ComplianceReportDto(
            stats.TotalEntries,
            byAction,
            byEntity,
            compliancePercent,
            retentionBreaches,
            stats.SensitiveCount,
            stats.OldestEntry,
            stats.NewestEntry));
    }
}
