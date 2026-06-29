using Mapster;
using MediatR;
using Obss.Audit.Application.Abstractions;
using Obss.Audit.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Queries.GetSensitiveOperations;

public sealed class GetSensitiveOperationsQueryHandler : IRequestHandler<GetSensitiveOperationsQuery, Result<IReadOnlyList<AuditEntryDto>>>
{
    private readonly IAuditEntryRepository _repository;

    public GetSensitiveOperationsQueryHandler(IAuditEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<AuditEntryDto>>> Handle(GetSensitiveOperationsQuery request, CancellationToken cancellationToken)
    {
        var entries = await _repository.GetSensitiveOperationsAsync(request.From, request.To, cancellationToken);
        var result = entries.Adapt<List<AuditEntryDto>>();
        return Result.Success<IReadOnlyList<AuditEntryDto>>(result);
    }
}
