using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.WirelessTransmission;

public sealed class ActivateWirelessTransmissionCommandHandler : IRequestHandler<ActivateWirelessTransmissionCommand, Result<WirelessTransmissionLifecycleResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IRepository<ProvisioningJob> _provisioningJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateWirelessTransmissionCommandHandler(
        IProductOrderRepository orderRepository,
        IRepository<ProvisioningJob> provisioningJobRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _provisioningJobRepository = provisioningJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WirelessTransmissionLifecycleResult>> Handle(ActivateWirelessTransmissionCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<WirelessTransmissionLifecycleResult>(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item is null)
            return Result.Failure<WirelessTransmissionLifecycleResult>(Error.NotFound("ProductOrderItem", request.OrderItemId));

        var jobId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var job = ProvisioningJob.Create(
            request.OrderId,
            request.OrderItemId,
            request.SubscriptionId,
            Guid.Empty,
            "WIRELESS_TRANSMISSION",
            ProvisioningAction.Activate);

        await _provisioningJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new WirelessTransmissionLifecycleResult(
            correlationId,
            "ActivationInProgress",
            $"Wireless transmission activation with {request.BandwidthMbps}Mbps, {request.AntennaType} antenna, range {request.RangeKm}km is in progress.",
            $"تفعيل النقل اللاسلكي بسعة {request.BandwidthMbps} ميجابت/ثانية وهوائي {request.AntennaType} ومدى {request.RangeKm} كم قيد التنفيذ.",
            jobId));
    }
}
