using Mapster;
using MediatR;
using Obss.Audit.Application.DTOs;
using Obss.Audit.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Commands.CreateAuditPolicy;

public sealed class CreateAuditPolicyCommandHandler : IRequestHandler<CreateAuditPolicyCommand, Result<AuditPolicyDto>>
{
    private readonly IRepository<AuditPolicy> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public CreateAuditPolicyCommandHandler(
        IRepository<AuditPolicy> repository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<Result<AuditPolicyDto>> Handle(CreateAuditPolicyCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId ?? string.Empty;

        var policy = AuditPolicy.Create(
            tenantId,
            request.EntityType,
            request.RetentionDays,
            request.AlertOnFailure,
            request.IsActive);

        await _repository.AddAsync(policy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(policy.Adapt<AuditPolicyDto>());
    }
}
