using FluentValidation;

namespace Obss.Orders.Application.Commands.ApproveProductOrder;

public sealed class ApproveProductOrderCommandValidator : AbstractValidator<ApproveProductOrderCommand>
{
    public ApproveProductOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");
    }
}
