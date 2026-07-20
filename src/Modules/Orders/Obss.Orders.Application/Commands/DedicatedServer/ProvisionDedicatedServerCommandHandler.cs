using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.DedicatedServer;

public sealed class ProvisionDedicatedServerCommandHandler : IRequestHandler<ProvisionDedicatedServerCommand, Result<DedicatedServerLifecycleResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IRepository<ProvisioningJob> _provisioningJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProvisionDedicatedServerCommandHandler(
        IProductOrderRepository orderRepository,
        IRepository<ProvisioningJob> provisioningJobRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _provisioningJobRepository = provisioningJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DedicatedServerLifecycleResult>> Handle(ProvisionDedicatedServerCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<DedicatedServerLifecycleResult>(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item is null)
            return Result.Failure<DedicatedServerLifecycleResult>(Error.NotFound("ProductOrderItem", request.OrderItemId));

        var jobId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var job = ProvisioningJob.Create(
            request.OrderId,
            request.OrderItemId,
            request.SubscriptionId,
            Guid.Empty,
            "DEDICATED_SERVER",
            ProvisioningAction.Provision);

        await _provisioningJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new DedicatedServerLifecycleResult(
            correlationId,
            "ProvisioningInProgress",
            $"Dedicated server '{request.Hostname}' provisioning is in progress. ({request.CpuCores} vCPU, {request.RamGb}GB RAM, {request.StorageGb}GB {request.StorageType})",
            $"توفير الخادم المخصص '{request.Hostname}' قيد التنفيذ. ({request.CpuCores} نواة، {request.RamGb} جيجابايت رام، {request.StorageGb} جيجابايت {request.StorageType})",
            jobId));
    }
}
