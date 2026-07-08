using Mapster;
using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.UpdateServiceOrder;

public sealed class UpdateServiceOrderCommandHandler : IRequestHandler<UpdateServiceOrderCommand, Result<ServiceOrderDto>>
{
    private readonly IServiceOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceOrderCommandHandler(IServiceOrderRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ServiceOrderDto>> Handle(UpdateServiceOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
            return Result.Failure<ServiceOrderDto>(Error.NotFound("ServiceOrder", request.Id));

        order.UpdateDetails(
            request.Description, request.Category, request.Priority,
            request.RequestedStartDate, request.RequestedCompletionDate);

        await _repository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(order.Adapt<ServiceOrderDto>());
    }
}
