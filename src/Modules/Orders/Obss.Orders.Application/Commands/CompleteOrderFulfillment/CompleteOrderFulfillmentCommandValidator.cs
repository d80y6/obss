using FluentValidation;

namespace Obss.Orders.Application.Commands.CompleteOrderFulfillment;

public sealed class CompleteOrderFulfillmentCommandValidator : AbstractValidator<CompleteOrderFulfillmentCommand>
{
    public CompleteOrderFulfillmentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        When(x => !x.IsSuccess, () =>
        {
            RuleFor(x => x.ErrorMessage)
                .NotEmpty().WithMessage("Error message is required when fulfillment failed.");
        });
    }
}
