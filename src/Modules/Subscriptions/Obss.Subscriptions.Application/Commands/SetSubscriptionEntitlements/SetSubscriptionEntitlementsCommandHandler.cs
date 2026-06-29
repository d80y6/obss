using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Domain.Entities;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Application.Commands.SetSubscriptionEntitlements;

public sealed class SetSubscriptionEntitlementsCommandHandler : IRequestHandler<SetSubscriptionEntitlementsCommand, Result>
{
    private readonly ISubscriptionEntitlementRepository _entitlementRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetSubscriptionEntitlementsCommandHandler(
        ISubscriptionEntitlementRepository entitlementRepository,
        ISubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork)
    {
        _entitlementRepository = entitlementRepository;
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SetSubscriptionEntitlementsCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken);

        if (subscription is null)
            return Result.Failure(Error.NotFound("Subscription", request.SubscriptionId));

        await _entitlementRepository.DeleteBySubscriptionIdAsync(request.SubscriptionId, cancellationToken);

        foreach (var entitlementDef in request.Entitlements)
        {
            if (!Enum.TryParse<EntitlementType>(entitlementDef.EntitlementType, out var entitlementType))
                return Result.Failure(Error.Validation($"Invalid entitlement type: '{entitlementDef.EntitlementType}'."));

            var entitlement = SubscriptionEntitlement.Create(
                request.SubscriptionId,
                entitlementType,
                entitlementDef.Name,
                entitlementDef.Limit,
                entitlementDef.Used,
                entitlementDef.Unit,
                entitlementDef.IsUnlimited,
                entitlementDef.IsOverridable,
                entitlementDef.ValidFrom,
                entitlementDef.ValidTo);

            await _entitlementRepository.AddAsync(entitlement, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
