using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Commands.UpdateDunningPolicy;

public sealed class UpdateDunningPolicyCommandHandler : IRequestHandler<UpdateDunningPolicyCommand, Result<DunningPolicyDto>>
{
    private readonly IDunningPolicyRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateDunningPolicyCommandHandler> _logger;

    public UpdateDunningPolicyCommandHandler(
        IDunningPolicyRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateDunningPolicyCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<DunningPolicyDto>> Handle(UpdateDunningPolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (policy is null)
            return Result.Failure<DunningPolicyDto>(Error.NotFound("DunningPolicy", request.Id));

        policy.UpdateDetails(
            request.Name,
            request.Description,
            request.MaxDunningLevel,
            request.DunningFees,
            request.DaysBetweenActions,
            request.EscalationAfterDays);

        if (request.IsActive && !policy.IsActive)
            policy.Activate();
        else if (!request.IsActive && policy.IsActive)
            policy.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Dunning policy {PolicyId} updated.",
            request.Id);

        return Result.Success(policy.Adapt<DunningPolicyDto>());
    }
}
