using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Lte;

public sealed class Suspend4GCommandHandler : IRequestHandler<Suspend4GCommand, Result<LifecycleResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IRepository<ProvisioningJob> _provisioningJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public Suspend4GCommandHandler(
        IProductOrderRepository orderRepository,
        IRepository<ProvisioningJob> provisioningJobRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _provisioningJobRepository = provisioningJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LifecycleResult>> Handle(Suspend4GCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<LifecycleResult>(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item is null)
            return Result.Failure<LifecycleResult>(Error.NotFound("ProductOrderItem", request.OrderItemId));

        if (!item.IsActive)
            return Result.Failure<LifecycleResult>(Error.Validation("Service is not active. Cannot suspend."));

        var jobId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var job = ProvisioningJob.Create(
            request.OrderId,
            request.OrderItemId,
            request.SubscriptionId,
            Guid.Empty,
            "LTE",
            ProvisioningAction.Suspend);

        await _provisioningJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new LifecycleResult(
            correlationId,
            "Suspended",
            $"4G service suspended successfully. Reason: {request.Reason}",
            $"تم تعليق خدمة 4G بنجاح. السبب: {request.Reason}",
            jobId));
    }
}
