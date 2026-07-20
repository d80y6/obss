using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.WirelessTransmission;

public sealed class TerminateWirelessTransmissionCommandHandler : IRequestHandler<TerminateWirelessTransmissionCommand, Result<WirelessTransmissionLifecycleResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IRepository<ProvisioningJob> _provisioningJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TerminateWirelessTransmissionCommandHandler(
        IProductOrderRepository orderRepository,
        IRepository<ProvisioningJob> provisioningJobRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _provisioningJobRepository = provisioningJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WirelessTransmissionLifecycleResult>> Handle(TerminateWirelessTransmissionCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<WirelessTransmissionLifecycleResult>(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item is null)
            return Result.Failure<WirelessTransmissionLifecycleResult>(Error.NotFound("ProductOrderItem", request.OrderItemId));

        if (!item.IsActive)
            return Result.Failure<WirelessTransmissionLifecycleResult>(Error.Validation("Service is already inactive. Cannot terminate."));

        var jobId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var job = ProvisioningJob.Create(
            request.OrderId,
            request.OrderItemId,
            request.SubscriptionId,
            Guid.Empty,
            "WIRELESS_TRANSMISSION",
            ProvisioningAction.Decommission);

        await _provisioningJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        item.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new WirelessTransmissionLifecycleResult(
            correlationId,
            "TerminationInProgress",
            $"Wireless transmission service termination is in progress. Reason: {request.Reason}",
            $"إنهاء خدمة النقل اللاسلكي قيد التنفيذ. السبب: {request.Reason}",
            jobId));
    }
}
