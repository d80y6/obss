using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Commands.Common;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.SuperShamel;

public sealed class TerminateSuperShamelCommandHandler : IRequestHandler<TerminateSuperShamelCommand, Result<LifecycleResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IRepository<ProvisioningJob> _provisioningJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TerminateSuperShamelCommandHandler(
        IProductOrderRepository orderRepository,
        IRepository<ProvisioningJob> provisioningJobRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _provisioningJobRepository = provisioningJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LifecycleResult>> Handle(TerminateSuperShamelCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<LifecycleResult>(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item is null)
            return Result.Failure<LifecycleResult>(Error.NotFound("ProductOrderItem", request.OrderItemId));

        if (!item.IsActive)
            return Result.Failure<LifecycleResult>(Error.Validation("Bundle is already inactive. Cannot terminate."));

        var correlationId = Guid.NewGuid();

        var ftthJob = ProvisioningJob.Create(
            request.OrderId, request.OrderItemId, request.SubscriptionId, Guid.Empty, "FTTH", ProvisioningAction.Decommission);

        var telephonyJob = ProvisioningJob.Create(
            request.OrderId, request.OrderItemId, request.SubscriptionId, Guid.Empty, "TELEPHONY", ProvisioningAction.Decommission);

        var lteJob = ProvisioningJob.Create(
            request.OrderId, request.OrderItemId, request.SubscriptionId, Guid.Empty, "LTE", ProvisioningAction.Decommission);

        await _provisioningJobRepository.AddAsync(ftthJob, cancellationToken);
        await _provisioningJobRepository.AddAsync(telephonyJob, cancellationToken);
        await _provisioningJobRepository.AddAsync(lteJob, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        item.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var componentResults = new List<ComponentResult>
        {
            new("FTTH", "الألياف الضوئية", "TerminationInProgress", "FTTH termination in progress.", "إنهاء الألياف الضوئية قيد التنفيذ.", ftthJob.Id),
            new("Hatif Tawasol", "هاتف تواصل", "TerminationInProgress", "Hatif Tawasol termination in progress.", "إنهاء هاتف تواصل قيد التنفيذ.", telephonyJob.Id),
            new("Yemen 4G", "يمن 4G", "TerminationInProgress", "Yemen 4G termination in progress.", "إنهاء يمن 4G قيد التنفيذ.", lteJob.Id)
        };

        return Result.Success(new LifecycleResult(
            correlationId,
            "TerminationInProgress",
            $"Super Shamel bundle termination is in progress. Reason: {request.Reason}",
            $"إنهاء باقة سوبر شامل قيد التنفيذ. السبب: {request.Reason}",
            null,
            componentResults.AsReadOnly()));
    }
}
