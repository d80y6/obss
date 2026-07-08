using MediatR;
using Obss.Provisioning.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.CompleteServiceOrderItem;

public sealed class CompleteServiceOrderItemCommandHandler : IRequestHandler<CompleteServiceOrderItemCommand, Result>
{
    private readonly IServiceOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteServiceOrderItemCommandHandler(IServiceOrderRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CompleteServiceOrderItemCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.ServiceOrderId, cancellationToken);
        if (order is null)
            return Result.Failure(Error.NotFound("ServiceOrder", request.ServiceOrderId));

        order.CompleteItem(request.ItemId, request.ServiceId);
        order.Complete();

        await _repository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
