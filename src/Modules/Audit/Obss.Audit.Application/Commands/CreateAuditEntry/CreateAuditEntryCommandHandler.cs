using Mapster;
using MediatR;
using Obss.Audit.Application.Abstractions;
using Obss.Audit.Application.DTOs;
using Obss.Audit.Domain.Entities;
using Obss.Audit.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Commands.CreateAuditEntry;

public sealed class CreateAuditEntryCommandHandler : IRequestHandler<CreateAuditEntryCommand, Result<AuditEntryDto>>
{
    private readonly IAuditEntryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public CreateAuditEntryCommandHandler(
        IAuditEntryRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<Result<AuditEntryDto>> Handle(CreateAuditEntryCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AuditAction>(request.Action, true, out var action))
        {
            return Result.Failure<AuditEntryDto>(Error.Validation($"Invalid audit action: '{request.Action}'."));
        }

        var tenantId = _currentTenant.TenantId ?? string.Empty;

        var entry = AuditEntry.Create(
            tenantId,
            request.EntityType,
            request.EntityId,
            action,
            request.Changes,
            request.PerformedById,
            request.PerformedByName,
            request.IpAddress,
            request.UserAgent,
            request.CorrelationId,
            request.IsSensitive);

        await _repository.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(entry.Adapt<AuditEntryDto>());
    }
}
