using FluentValidation;

namespace Obss.Orders.Application.Commands.DeleteOrder;

internal sealed class DeleteOrderValidator : AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Order ID is required.");
    }
}
