using Mapster;
using MediatR;
using Obss.Audit.Application.Abstractions;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Queries.GetEntityAuditTrail;

public sealed class GetEntityAuditTrailQueryHandler : IRequestHandler<GetEntityAuditTrailQuery, Result<IReadOnlyList<AuditEntryDto>>>
{
    private readonly IAuditEntryRepository _repository;
    private readonly ICurrentTenant _currentTenant;

    public GetEntityAuditTrailQueryHandler(IAuditEntryRepository repository, ICurrentTenant currentTenant)
    {
        _repository = repository;
        _currentTenant = currentTenant;
    }

    public async Task<Result<IReadOnlyList<AuditEntryDto>>> Handle(GetEntityAuditTrailQuery request, CancellationToken cancellationToken)
    {
        var entries = await _repository.GetEntityTrailAsync(
            _currentTenant.TenantId ?? string.Empty,
            request.EntityType,
            request.EntityId,
            cancellationToken);

        var result = entries.Adapt<List<AuditEntryDto>>();
        return Result.Success<IReadOnlyList<AuditEntryDto>>(result);
    }
}
