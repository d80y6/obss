using Mapster;
using MediatR;
using Obss.Audit.Application.Abstractions;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Queries.GetAuditEntries;

public sealed class GetAuditEntriesQueryHandler : IRequestHandler<GetAuditEntriesQuery, Result<IReadOnlyList<AuditEntryDto>>>
{
    private readonly IAuditEntryRepository _repository;
    private readonly ICurrentTenant _currentTenant;

    public GetAuditEntriesQueryHandler(IAuditEntryRepository repository, ICurrentTenant currentTenant)
    {
        _repository = repository;
        _currentTenant = currentTenant;
    }

    public async Task<Result<IReadOnlyList<AuditEntryDto>>> Handle(GetAuditEntriesQuery request, CancellationToken cancellationToken)
    {
        var entries = await _repository.GetFilteredAsync(
            _currentTenant.TenantId,
            request.EntityType,
            request.EntityId,
            request.Action,
            request.PerformedById,
            request.FromDate,
            request.ToDate,
            request.Page,
            request.PageSize,
            cancellationToken);

        var result = entries.Adapt<List<AuditEntryDto>>();
        return Result.Success<IReadOnlyList<AuditEntryDto>>(result);
    }
}
