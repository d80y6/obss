using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Orders.Application.Abstractions;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Domain.Events;
using Obss.Provisioning.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.ServiceInventory.Application.Abstractions;

namespace Obss.Provisioning.Application.DomainEventHandlers;

public sealed class ProvisioningJobCompletedEventHandler : INotificationHandler<ProvisioningJobCompletedDomainEvent>
{
    private readonly IProvisioningJobRepository _jobRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IProductOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProvisioningJobCompletedEventHandler> _logger;

    public ProvisioningJobCompletedEventHandler(
        IProvisioningJobRepository jobRepository,
        IServiceRepository serviceRepository,
        IProductOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProvisioningJobCompletedEventHandler> logger)
    {
        _jobRepository = jobRepository;
        _serviceRepository = serviceRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(ProvisioningJobCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(notification.JobId, cancellationToken);
        if (job is null)
            return;

        if ((job.Action == Domain.ValueObjects.ProvisioningAction.Provision ||
             job.Action == Domain.ValueObjects.ProvisioningAction.Activate) &&
            job.ServiceId.HasValue)
        {
            var service = await _serviceRepository.GetByIdAsync(job.ServiceId.Value, cancellationToken);
            if (service is not null)
            {
                service.Activate();
            }
        }

        if (job.OrderId != Guid.Empty)
        {
            var order = await _orderRepository.GetByIdWithItemsAsync(job.OrderId, cancellationToken);
            if (order?.Fulfillment is not null)
            {
                order.Fulfillment.Complete();
                _logger.LogInformation(
                    "Completed fulfillment {FulfillmentId} for order {OrderId} after provisioning job {JobId} completed",
                    order.Fulfillment.Id, job.OrderId, job.Id);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
