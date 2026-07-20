using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Commands.Common;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.SuperShamel;

public sealed class ChangeSuperShamelPlanCommandHandler : IRequestHandler<ChangeSuperShamelPlanCommand, Result<LifecycleResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IRepository<ProvisioningJob> _provisioningJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeSuperShamelPlanCommandHandler(
        IProductOrderRepository orderRepository,
        IRepository<ProvisioningJob> provisioningJobRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _provisioningJobRepository = provisioningJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LifecycleResult>> Handle(ChangeSuperShamelPlanCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<LifecycleResult>(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item is null)
            return Result.Failure<LifecycleResult>(Error.NotFound("ProductOrderItem", request.OrderItemId));

        var correlationId = Guid.NewGuid();
        var componentResults = new List<ComponentResult>();

        if (request.NewFtthSpeedMbps.HasValue)
        {
            var ftthJob = ProvisioningJob.Create(
                request.OrderId, request.OrderItemId, request.SubscriptionId, Guid.Empty, "FTTH", ProvisioningAction.Change);
            await _provisioningJobRepository.AddAsync(ftthJob, cancellationToken);
            componentResults.Add(new("FTTH", "الألياف الضوئية", "ChangeInProgress", $"FTTH speed change to {request.NewFtthSpeedMbps}Mbps in progress.", $"تغيير سرعة الألياف الضوئية إلى {request.NewFtthSpeedMbps} ميجابت/ثانية قيد التنفيذ.", ftthJob.Id));
        }

        if (request.NewHatifTawasolPackage is not null)
        {
            var telephonyJob = ProvisioningJob.Create(
                request.OrderId, request.OrderItemId, request.SubscriptionId, Guid.Empty, "TELEPHONY", ProvisioningAction.Change);
            await _provisioningJobRepository.AddAsync(telephonyJob, cancellationToken);
            componentResults.Add(new("Hatif Tawasol", "هاتف تواصل", "ChangeInProgress", "Hatif Tawasol package change in progress.", "تغيير باقة هاتف تواصل قيد التنفيذ.", telephonyJob.Id));
        }

        if (request.NewYemen4GDataPlan is not null)
        {
            var lteJob = ProvisioningJob.Create(
                request.OrderId, request.OrderItemId, request.SubscriptionId, Guid.Empty, "LTE", ProvisioningAction.Change);
            await _provisioningJobRepository.AddAsync(lteJob, cancellationToken);
            componentResults.Add(new("Yemen 4G", "يمن 4G", "ChangeInProgress", "Yemen 4G plan change in progress.", "تغيير باقة يمن 4G قيد التنفيذ.", lteJob.Id));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new LifecycleResult(
            correlationId,
            "ChangeInProgress",
            "Super Shamel bundle plan change is in progress.",
            "تغيير باقة سوبر شامل قيد التنفيذ.",
            null,
            componentResults.AsReadOnly()));
    }
}
