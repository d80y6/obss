using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.DTOs;
using Obss.Provisioning.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Provisioning.Application.Commands.CreateServiceOrder;

public sealed class CreateServiceOrderCommandHandler : IRequestHandler<CreateServiceOrderCommand, Result<ServiceOrderDto>>
{
    private readonly IServiceOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateServiceOrderCommandHandler> _logger;

    public CreateServiceOrderCommandHandler(
        IServiceOrderRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<CreateServiceOrderCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ServiceOrderDto>> Handle(CreateServiceOrderCommand request, CancellationToken cancellationToken)
    {
        var order = ServiceOrder.Create(
            request.TenantId,
            request.ExternalId,
            request.Description,
            request.Category,
            request.Priority);

        foreach (var itemRequest in request.Items)
        {
            if (!Enum.TryParse<ServiceOrderAction>(itemRequest.Action, out var action))
                return Result.Failure<ServiceOrderDto>(Error.Validation($"Invalid action: '{itemRequest.Action}'"));

            order.AddItem(
                itemRequest.ServiceId,
                action,
                itemRequest.Quantity,
                itemRequest.Description,
                itemRequest.RequestedStartDate,
                itemRequest.RequestedCompletionDate);
        }

        await _repository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created ServiceOrder {Id} with {ItemCount} items", order.Id, order.Items.Count);

        return Result.Success(order.Adapt<ServiceOrderDto>());
    }
}
