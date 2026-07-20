using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Commands.Common;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.SuperShamel;

public sealed class SubscribeSuperShamelCommandHandler : IRequestHandler<SubscribeSuperShamelCommand, Result<LifecycleResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IRepository<ProvisioningJob> _provisioningJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubscribeSuperShamelCommandHandler(
        IProductOrderRepository orderRepository,
        IRepository<ProvisioningJob> provisioningJobRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _provisioningJobRepository = provisioningJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LifecycleResult>> Handle(SubscribeSuperShamelCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<LifecycleResult>(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item is null)
            return Result.Failure<LifecycleResult>(Error.NotFound("ProductOrderItem", request.OrderItemId));

        var correlationId = Guid.NewGuid();

        var ftthJob = ProvisioningJob.Create(
            request.OrderId, request.OrderItemId, request.SubscriptionId, Guid.Empty, "FTTH", ProvisioningAction.Activate);

        var telephonyJob = ProvisioningJob.Create(
            request.OrderId, request.OrderItemId, request.SubscriptionId, Guid.Empty, "TELEPHONY", ProvisioningAction.Activate);

        var lteJob = ProvisioningJob.Create(
            request.OrderId, request.OrderItemId, request.SubscriptionId, Guid.Empty, "LTE", ProvisioningAction.Activate);

        await _provisioningJobRepository.AddAsync(ftthJob, cancellationToken);
        await _provisioningJobRepository.AddAsync(telephonyJob, cancellationToken);
        await _provisioningJobRepository.AddAsync(lteJob, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var componentResults = new List<ComponentResult>
        {
            new("FTTH", "الألياف الضوئية", "ActivationInProgress", "FTTH activation is in progress.", "تفعيل الألياف الضوئية قيد التنفيذ.", ftthJob.Id),
            new("Hatif Tawasol", "هاتف تواصل", "ActivationInProgress", "Hatif Tawasol activation is in progress.", "تفعيل هاتف تواصل قيد التنفيذ.", telephonyJob.Id),
            new("Yemen 4G", "يمن 4G", "ActivationInProgress", "Yemen 4G activation is in progress.", "تفعيل يمن 4G قيد التنفيذ.", lteJob.Id)
        };

        return Result.Success(new LifecycleResult(
            correlationId,
            "SubscriptionInProgress",
            "Super Shamel bundle subscription is in progress.",
            "الاشتراك في باقة سوبر شامل قيد التنفيذ.",
            null,
            componentResults.AsReadOnly()));
    }
}
