using FluentValidation;

namespace Obss.Orders.Application.Commands.ApproveOrder;

internal sealed class ApproveOrderCommandValidator : AbstractValidator<ApproveOrderCommand>
{
    public ApproveOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");
    }
}
