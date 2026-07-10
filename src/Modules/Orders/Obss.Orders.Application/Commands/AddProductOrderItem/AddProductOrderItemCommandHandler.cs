using MediatR;
using Obss.Orders.Application.Abstractions;
using Obss.Orders.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Orders.Application.Commands.AddProductOrderItem;

public sealed class AddProductOrderItemCommandHandler : IRequestHandler<AddProductOrderItemCommand, Result>
{
    private readonly IProductOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddProductOrderItemCommandHandler(IProductOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddProductOrderItemCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            return Result.Failure(Error.NotFound("ProductOrder", request.OrderId));

        if (!Enum.TryParse<BillingPeriod>(request.BillingPeriod, true, out var billingPeriod))
            return Result.Failure(Error.Validation($"Invalid billing period: {request.BillingPeriod}"));

        try
        {
            order.AddItem(
                request.ProductId,
                request.OfferId,
                request.ProductName,
                request.OfferName,
                request.Quantity,
                request.UnitPrice,
                request.RecurringPrice,
                request.DiscountAmount,
                request.TaxAmount,
                billingPeriod,
                request.StartDate,
                request.EndDate);

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
