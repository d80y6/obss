using Mapster;
using MediatR;
using Obss.Audit.Application.Abstractions;
using Obss.Audit.Application.DTOs;
using Obss.Audit.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Queries.GetAuditEntryById;

public sealed class GetAuditEntryByIdQueryHandler : IRequestHandler<GetAuditEntryByIdQuery, Result<AuditEntryDto>>
{
    private readonly IAuditEntryRepository _repository;

    public GetAuditEntryByIdQueryHandler(IAuditEntryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AuditEntryDto>> Handle(GetAuditEntryByIdQuery request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entry is null)
            return Result.Failure<AuditEntryDto>(Error.NotFound(nameof(AuditEntry), request.Id));

        return Result.Success(entry.Adapt<AuditEntryDto>());
    }
}
