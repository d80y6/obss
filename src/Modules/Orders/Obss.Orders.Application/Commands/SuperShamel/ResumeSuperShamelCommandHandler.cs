using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Application.Commands.Common;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.SuperShamel;

public sealed class ResumeSuperShamelCommandHandler : IRequestHandler<ResumeSuperShamelCommand, Result<LifecycleResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IRepository<ProvisioningJob> _provisioningJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResumeSuperShamelCommandHandler(
        IProductOrderRepository orderRepository,
        IRepository<ProvisioningJob> provisioningJobRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _provisioningJobRepository = provisioningJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LifecycleResult>> Handle(ResumeSuperShamelCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<LifecycleResult>(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item is null)
            return Result.Failure<LifecycleResult>(Error.NotFound("ProductOrderItem", request.OrderItemId));

        var correlationId = Guid.NewGuid();

        var ftthJob = ProvisioningJob.Create(
            request.OrderId, request.OrderItemId, request.SubscriptionId, Guid.Empty, "FTTH", ProvisioningAction.Resume);

        var telephonyJob = ProvisioningJob.Create(
            request.OrderId, request.OrderItemId, request.SubscriptionId, Guid.Empty, "TELEPHONY", ProvisioningAction.Resume);

        var lteJob = ProvisioningJob.Create(
            request.OrderId, request.OrderItemId, request.SubscriptionId, Guid.Empty, "LTE", ProvisioningAction.Resume);

        await _provisioningJobRepository.AddAsync(ftthJob, cancellationToken);
        await _provisioningJobRepository.AddAsync(telephonyJob, cancellationToken);
        await _provisioningJobRepository.AddAsync(lteJob, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var componentResults = new List<ComponentResult>
        {
            new("FTTH", "الألياف الضوئية", "ResumeInProgress", "FTTH resume in progress.", "استئناف الألياف الضوئية قيد التنفيذ.", ftthJob.Id),
            new("Hatif Tawasol", "هاتف تواصل", "ResumeInProgress", "Hatif Tawasol resume in progress.", "استئناف هاتف تواصل قيد التنفيذ.", telephonyJob.Id),
            new("Yemen 4G", "يمن 4G", "ResumeInProgress", "Yemen 4G resume in progress.", "استئناف يمن 4G قيد التنفيذ.", lteJob.Id)
        };

        return Result.Success(new LifecycleResult(
            correlationId,
            "ResumeInProgress",
            "Super Shamel bundle resumption is in progress.",
            "استئناف باقة سوبر شامل قيد التنفيذ.",
            null,
            componentResults.AsReadOnly()));
    }
}
