using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Application.DTOs;
using Obss.Collections.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Commands.CreateDunningPolicy;

public sealed class CreateDunningPolicyCommandHandler : IRequestHandler<CreateDunningPolicyCommand, Result<DunningPolicyDto>>
{
    private readonly IDunningPolicyRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<CreateDunningPolicyCommandHandler> _logger;

    public CreateDunningPolicyCommandHandler(
        IDunningPolicyRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant,
        ILogger<CreateDunningPolicyCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task<Result<DunningPolicyDto>> Handle(CreateDunningPolicyCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenant.TenantId;
        if (string.IsNullOrEmpty(tenantId))
            return Result.Failure<DunningPolicyDto>(Error.Unauthorized("Tenant context is required."));

        var policy = DunningPolicy.Create(
            tenantId,
            request.Name,
            request.Description,
            request.MaxDunningLevel,
            request.DunningFees,
            request.DaysBetweenActions,
            request.EscalationAfterDays);

        await _repository.AddAsync(policy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Dunning policy {PolicyId} '{Name}' created with max level {MaxLevel}.",
            policy.Id,
            request.Name,
            request.MaxDunningLevel);

        return Result.Success(policy.Adapt<DunningPolicyDto>());
    }
}
