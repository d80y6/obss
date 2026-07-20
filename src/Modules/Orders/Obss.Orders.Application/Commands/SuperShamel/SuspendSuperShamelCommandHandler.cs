using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Commands.Common;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.SuperShamel;

public sealed class SuspendSuperShamelCommandHandler : IRequestHandler<SuspendSuperShamelCommand, Result<LifecycleResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IRepository<ProvisioningJob> _provisioningJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SuspendSuperShamelCommandHandler(
        IProductOrderRepository orderRepository,
        IRepository<ProvisioningJob> provisioningJobRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _provisioningJobRepository = provisioningJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LifecycleResult>> Handle(SuspendSuperShamelCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<LifecycleResult>(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item is null)
            return Result.Failure<LifecycleResult>(Error.NotFound("ProductOrderItem", request.OrderItemId));

        if (!item.IsActive)
            return Result.Failure<LifecycleResult>(Error.Validation("Bundle is not active. Cannot suspend."));

        var correlationId = Guid.NewGuid();

        var ftthJob = ProvisioningJob.Create(
            request.OrderId, request.OrderItemId, request.SubscriptionId, Guid.Empty, "FTTH", ProvisioningAction.Suspend);

        var telephonyJob = ProvisioningJob.Create(
            request.OrderId, request.OrderItemId, request.SubscriptionId, Guid.Empty, "TELEPHONY", ProvisioningAction.Suspend);

        var lteJob = ProvisioningJob.Create(
            request.OrderId, request.OrderItemId, request.SubscriptionId, Guid.Empty, "LTE", ProvisioningAction.Suspend);

        await _provisioningJobRepository.AddAsync(ftthJob, cancellationToken);
        await _provisioningJobRepository.AddAsync(telephonyJob, cancellationToken);
        await _provisioningJobRepository.AddAsync(lteJob, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var componentResults = new List<ComponentResult>
        {
            new("FTTH", "الألياف الضوئية", "SuspendInProgress", "FTTH suspend in progress.", "تعليق الألياف الضوئية قيد التنفيذ.", ftthJob.Id),
            new("Hatif Tawasol", "هاتف تواصل", "SuspendInProgress", "Hatif Tawasol suspend in progress.", "تعليق هاتف تواصل قيد التنفيذ.", telephonyJob.Id),
            new("Yemen 4G", "يمن 4G", "SuspendInProgress", "Yemen 4G suspend in progress.", "تعليق يمن 4G قيد التنفيذ.", lteJob.Id)
        };

        return Result.Success(new LifecycleResult(
            correlationId,
            "SuspendInProgress",
            $"Super Shamel bundle suspension is in progress. Reason: {request.Reason}",
            $"تعليق باقة سوبر شامل قيد التنفيذ. السبب: {request.Reason}",
            null,
            componentResults.AsReadOnly()));
    }
}
