using FluentValidation;

namespace Obss.ServiceInventory.Application.Commands.CreateService;

public sealed class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.ServiceType)
            .NotEmpty().WithMessage("Service type is required.")
            .MaximumLength(50).WithMessage("Service type must not exceed 50 characters.");

        RuleFor(x => x.ServiceIdentifier)
            .NotEmpty().WithMessage("Service identifier is required.")
            .MaximumLength(200).WithMessage("Service identifier must not exceed 200 characters.");

        RuleFor(x => x.Location)
            .MaximumLength(500).WithMessage("Location must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Location));

        RuleFor(x => x.Configuration)
            .MaximumLength(4000).WithMessage("Configuration must not exceed 4000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Configuration));
    }
}
