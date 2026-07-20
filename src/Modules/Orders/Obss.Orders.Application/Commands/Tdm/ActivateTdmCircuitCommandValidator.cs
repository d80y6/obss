using FluentValidation;

namespace Obss.Orders.Application.Commands.Tdm;

public sealed class ActivateTdmCircuitCommandValidator : AbstractValidator<ActivateTdmCircuitCommand>
{
    public ActivateTdmCircuitCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.CircuitType)
            .NotEmpty().WithMessage("Circuit type is required.")
            .Must(x => x is "E1" or "STM-1" or "STM-4" or "STM-16")
            .WithMessage("Circuit type must be 'E1', 'STM-1', 'STM-4', or 'STM-16'.");

        RuleFor(x => x.EndpointA)
            .NotEmpty().WithMessage("Endpoint A is required.")
            .MaximumLength(200).WithMessage("Endpoint A must not exceed 200 characters.");

        RuleFor(x => x.EndpointB)
            .NotEmpty().WithMessage("Endpoint B is required.")
            .MaximumLength(200).WithMessage("Endpoint B must not exceed 200 characters.");

        RuleFor(x => x.ChannelCount)
            .InclusiveBetween(1, 1024).WithMessage("Channel count must be between 1 and 1024.");

        RuleFor(x => x.Framing)
            .NotEmpty().WithMessage("Framing is required.")
            .MaximumLength(50).WithMessage("Framing must not exceed 50 characters.");

        RuleFor(x => x.LineCoding)
            .NotEmpty().WithMessage("Line coding is required.")
            .MaximumLength(50).WithMessage("Line coding must not exceed 50 characters.");
    }
}
