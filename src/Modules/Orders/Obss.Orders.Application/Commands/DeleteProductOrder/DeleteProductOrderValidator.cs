using FluentValidation;

namespace Obss.Orders.Application.Commands.DeleteProductOrder;

public sealed class DeleteProductOrderValidator : AbstractValidator<DeleteProductOrderCommand>
{
    public DeleteProductOrderValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Order ID is required.");
    }
}
