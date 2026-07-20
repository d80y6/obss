using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.HomeWireless;

public sealed class ResumeHomeWirelessCommandHandler : IRequestHandler<ResumeHomeWirelessCommand, Result<HomeWirelessLifecycleResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IRepository<ProvisioningJob> _provisioningJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResumeHomeWirelessCommandHandler(
        IProductOrderRepository orderRepository,
        IRepository<ProvisioningJob> provisioningJobRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _provisioningJobRepository = provisioningJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<HomeWirelessLifecycleResult>> Handle(ResumeHomeWirelessCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<HomeWirelessLifecycleResult>(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item is null)
            return Result.Failure<HomeWirelessLifecycleResult>(Error.NotFound("ProductOrderItem", request.OrderItemId));

        var jobId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var job = ProvisioningJob.Create(
            request.OrderId,
            request.OrderItemId,
            request.SubscriptionId,
            Guid.Empty,
            "HOME_WIRELESS",
            ProvisioningAction.Resume);

        await _provisioningJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new HomeWirelessLifecycleResult(
            correlationId,
            "Resumed",
            "Home Wireless 4G service resumed successfully.",
            "تم استئناف خدمة Home Wireless 4G بنجاح.",
            jobId));
    }
}
