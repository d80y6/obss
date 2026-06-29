using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Domain.ValueObjects;

namespace Obss.Subscriptions.Application.Commands.UpdateEntitlementUsage;

public sealed class UpdateEntitlementUsageCommandHandler : IRequestHandler<UpdateEntitlementUsageCommand, Result>
{
    private readonly ISubscriptionEntitlementRepository _entitlementRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEntitlementUsageCommandHandler(
        ISubscriptionEntitlementRepository entitlementRepository,
        IUnitOfWork unitOfWork)
    {
        _entitlementRepository = entitlementRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateEntitlementUsageCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<EntitlementType>(request.EntitlementType, out var entitlementType))
            return Result.Failure(Error.Validation($"Invalid entitlement type: '{request.EntitlementType}'."));

        var entitlement = await _entitlementRepository.GetBySubscriptionAndTypeAsync(
            request.SubscriptionId, entitlementType, cancellationToken);

        if (entitlement is null)
            return Result.Failure(Error.NotFound("SubscriptionEntitlement", $"{request.SubscriptionId}/{request.EntitlementType}"));

        if (request.IsReduction)
            entitlement.ReduceUsage(request.Amount);
        else
            entitlement.RecordUsage(request.Amount);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
