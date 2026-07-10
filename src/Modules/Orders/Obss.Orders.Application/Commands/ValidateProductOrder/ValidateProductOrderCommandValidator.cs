using FluentValidation;

namespace Obss.Orders.Application.Commands.ValidateProductOrder;

public sealed class ValidateProductOrderCommandValidator : AbstractValidator<ValidateProductOrderCommand>
{
    public ValidateProductOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();
    }
}
