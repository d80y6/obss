using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.ApproveOrder;

public sealed class ApproveOrderCommandHandler : IRequestHandler<ApproveOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveOrderCommandHandler(
        IOrderRepository orderRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ApproveOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(Error.NotFound("Order", request.OrderId));

        var userId = _currentUser.UserId ?? "system";

        try
        {
            order.Approve(userId);
            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }
    }
}
