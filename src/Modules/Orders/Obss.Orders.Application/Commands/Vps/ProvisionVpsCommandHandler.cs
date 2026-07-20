using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Vps;

public sealed class ProvisionVpsCommandHandler : IRequestHandler<ProvisionVpsCommand, Result<VpsLifecycleResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IRepository<ProvisioningJob> _provisioningJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProvisionVpsCommandHandler(
        IProductOrderRepository orderRepository,
        IRepository<ProvisioningJob> provisioningJobRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _provisioningJobRepository = provisioningJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<VpsLifecycleResult>> Handle(ProvisionVpsCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<VpsLifecycleResult>(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item is null)
            return Result.Failure<VpsLifecycleResult>(Error.NotFound("ProductOrderItem", request.OrderItemId));

        var jobId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var job = ProvisioningJob.Create(
            request.OrderId,
            request.OrderItemId,
            request.SubscriptionId,
            Guid.Empty,
            "VPS",
            ProvisioningAction.Provision);

        await _provisioningJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new VpsLifecycleResult(
            correlationId,
            "ProvisioningInProgress",
            $"VPS '{request.Hostname}' provisioning is in progress. ({request.CpuCores} vCPU, {request.RamMb}MB RAM, {request.StorageGb}GB storage)",
            $"توفير الخادم الافتراضي '{request.Hostname}' قيد التنفيذ. ({request.CpuCores} نواة، {request.RamMb} ميجابايت رام، {request.StorageGb} جيجابايت تخزين)",
            jobId));
    }
}
