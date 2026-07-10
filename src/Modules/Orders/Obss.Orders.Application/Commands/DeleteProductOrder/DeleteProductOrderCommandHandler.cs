using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.DeleteProductOrder;

public sealed class DeleteProductOrderCommandHandler : IRequestHandler<DeleteProductOrderCommand, Result>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductOrderCommandHandler(IProductOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteProductOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
            return Result.Failure(Error.NotFound("ProductOrder", request.Id));

        if (order.Status != OrderStatus.Draft)
            return Result.Failure(Error.Validation("Only draft orders can be deleted."));

        await _orderRepository.DeleteAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
