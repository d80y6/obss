using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.AtmConnectivity;

public sealed class TerminateAtmConnectionCommandHandler : IRequestHandler<TerminateAtmConnectionCommand, Result<AtmLifecycleResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IRepository<ProvisioningJob> _provisioningJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TerminateAtmConnectionCommandHandler(
        IProductOrderRepository orderRepository,
        IRepository<ProvisioningJob> provisioningJobRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _provisioningJobRepository = provisioningJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AtmLifecycleResult>> Handle(TerminateAtmConnectionCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<AtmLifecycleResult>(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item is null)
            return Result.Failure<AtmLifecycleResult>(Error.NotFound("ProductOrderItem", request.OrderItemId));

        if (!item.IsActive)
            return Result.Failure<AtmLifecycleResult>(Error.Validation("Connection is already inactive. Cannot terminate."));

        var jobId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var job = ProvisioningJob.Create(
            request.OrderId,
            request.OrderItemId,
            request.SubscriptionId,
            Guid.Empty,
            "ATM_CONNECTIVITY",
            ProvisioningAction.Decommission);

        await _provisioningJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        item.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new AtmLifecycleResult(
            correlationId,
            "TerminationInProgress",
            $"ATM connection termination is in progress. Reason: {request.Reason}",
            $"إنهاء اتصال الصراف الآلي قيد التنفيذ. السبب: {request.Reason}",
            jobId));
    }
}
