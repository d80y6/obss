using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.StaticIp;

public sealed class ChangeStaticIpCommandHandler : IRequestHandler<ChangeStaticIpCommand, Result<StaticIpLifecycleResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IRepository<ProvisioningJob> _provisioningJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeStaticIpCommandHandler(
        IProductOrderRepository orderRepository,
        IRepository<ProvisioningJob> provisioningJobRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _provisioningJobRepository = provisioningJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StaticIpLifecycleResult>> Handle(ChangeStaticIpCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<StaticIpLifecycleResult>(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item is null)
            return Result.Failure<StaticIpLifecycleResult>(Error.NotFound("ProductOrderItem", request.OrderItemId));

        var jobId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var job = ProvisioningJob.Create(
            request.OrderId,
            request.OrderItemId,
            request.SubscriptionId,
            Guid.Empty,
            "STATIC_IP",
            ProvisioningAction.Change);

        await _provisioningJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new StaticIpLifecycleResult(
            correlationId,
            "ChangeInProgress",
            "Static IP configuration change is in progress.",
            "تغيير تكوين عنوان IP الثابت قيد التنفيذ.",
            jobId));
    }
}
