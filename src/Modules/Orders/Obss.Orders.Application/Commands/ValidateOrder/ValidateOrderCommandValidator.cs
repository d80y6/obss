using FluentValidation;

namespace Obss.Orders.Application.Commands.ValidateOrder;

public sealed class ValidateOrderCommandValidator : AbstractValidator<ValidateOrderCommand>
{
    public ValidateOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();
    }
}
