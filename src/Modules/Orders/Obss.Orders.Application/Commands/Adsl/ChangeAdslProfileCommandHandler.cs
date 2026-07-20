using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Adsl;

public sealed class ChangeAdslProfileCommandHandler : IRequestHandler<ChangeAdslProfileCommand, Result<AdslLifecycleResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IRepository<ProvisioningJob> _provisioningJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeAdslProfileCommandHandler(
        IProductOrderRepository orderRepository,
        IRepository<ProvisioningJob> provisioningJobRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _provisioningJobRepository = provisioningJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AdslLifecycleResult>> Handle(ChangeAdslProfileCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<AdslLifecycleResult>(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item is null)
            return Result.Failure<AdslLifecycleResult>(Error.NotFound("ProductOrderItem", request.OrderItemId));

        if (request.DownstreamSpeedKbps <= 0)
            return Result.Failure<AdslLifecycleResult>(Error.Validation("Downstream speed must be greater than zero."));

        if (request.UpstreamSpeedKbps <= 0)
            return Result.Failure<AdslLifecycleResult>(Error.Validation("Upstream speed must be greater than zero."));

        var jobId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var job = ProvisioningJob.Create(
            request.OrderId,
            request.OrderItemId,
            request.SubscriptionId,
            Guid.Empty,
            "ADSL",
            ProvisioningAction.Change);

        await _provisioningJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new AdslLifecycleResult(
            correlationId,
            "ProfileChangeInProgress",
            $"ADSL profile change to {request.LineProfile} ({request.DownstreamSpeedKbps}/{request.UpstreamSpeedKbps} Kbps) is in progress.",
            $"تغيير ملف ADSL إلى {request.LineProfile} ({request.DownstreamSpeedKbps}/{request.UpstreamSpeedKbps} كيلوبت/ثانية) قيد التنفيذ.",
            jobId));
    }
}
