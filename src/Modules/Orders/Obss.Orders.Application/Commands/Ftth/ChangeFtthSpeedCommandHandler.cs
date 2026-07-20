using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.Ftth;

public sealed class ChangeFtthSpeedCommandHandler : IRequestHandler<ChangeFtthSpeedCommand, Result<FtthLifecycleResult>>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IRepository<ProvisioningJob> _provisioningJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeFtthSpeedCommandHandler(
        IProductOrderRepository orderRepository,
        IRepository<ProvisioningJob> provisioningJobRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _provisioningJobRepository = provisioningJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FtthLifecycleResult>> Handle(ChangeFtthSpeedCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure<FtthLifecycleResult>(Error.NotFound("ProductOrder", request.OrderId));

        var item = order.Items.FirstOrDefault(i => i.Id == request.OrderItemId);
        if (item is null)
            return Result.Failure<FtthLifecycleResult>(Error.NotFound("ProductOrderItem", request.OrderItemId));

        if (request.NewDownloadSpeedMbps <= 0)
            return Result.Failure<FtthLifecycleResult>(Error.Validation("Download speed must be greater than zero."));

        if (request.NewUploadSpeedMbps <= 0)
            return Result.Failure<FtthLifecycleResult>(Error.Validation("Upload speed must be greater than zero."));

        var jobId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var job = ProvisioningJob.Create(
            request.OrderId,
            request.OrderItemId,
            request.SubscriptionId,
            Guid.Empty,
            "FTTH",
            ProvisioningAction.Change);

        await _provisioningJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new FtthLifecycleResult(
            correlationId,
            "SpeedChangeInProgress",
            $"FTTH speed change to {request.NewDownloadSpeedMbps}/{request.NewUploadSpeedMbps}Mbps is in progress.",
            $"تغيير سرعة الألياف الضوئية إلى {request.NewDownloadSpeedMbps}/{request.NewUploadSpeedMbps} ميجابت/ثانية قيد التنفيذ.",
            jobId));
    }
}
