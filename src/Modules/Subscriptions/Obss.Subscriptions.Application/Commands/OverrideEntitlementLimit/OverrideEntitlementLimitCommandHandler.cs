using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Application.Commands.OverrideEntitlementLimit;

public sealed class OverrideEntitlementLimitCommandHandler : IRequestHandler<OverrideEntitlementLimitCommand, Result>
{
    private readonly ISubscriptionEntitlementRepository _entitlementRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OverrideEntitlementLimitCommandHandler(
        ISubscriptionEntitlementRepository entitlementRepository,
        IUnitOfWork unitOfWork)
    {
        _entitlementRepository = entitlementRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(OverrideEntitlementLimitCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<EntitlementType>(request.EntitlementType, out var entitlementType))
            return Result.Failure(Error.Validation($"Invalid entitlement type: '{request.EntitlementType}'."));

        var entitlement = await _entitlementRepository.GetBySubscriptionAndTypeAsync(
            request.SubscriptionId, entitlementType, cancellationToken);

        if (entitlement is null)
            return Result.Failure(Error.NotFound("SubscriptionEntitlement", $"{request.SubscriptionId}/{request.EntitlementType}"));

        if (!entitlement.IsOverridable)
            return Result.Failure(Error.Validation($"Entitlement '{request.EntitlementType}' is not overridable."));

        entitlement.UpdateLimit(request.NewLimit);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
