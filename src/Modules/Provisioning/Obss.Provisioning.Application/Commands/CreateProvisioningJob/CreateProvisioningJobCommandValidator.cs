using FluentValidation;

namespace Obss.Provisioning.Application.Commands.CreateProvisioningJob;

public sealed class CreateProvisioningJobCommandValidator : AbstractValidator<CreateProvisioningJobCommand>
{
    public CreateProvisioningJobCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.ServiceType)
            .NotEmpty().WithMessage("Service type is required.")
            .MaximumLength(100);

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required.")
            .Must(a => a is "Provision" or "Activate" or "Suspend" or "Resume" or "Decommission" or "Change")
            .WithMessage("Action must be one of: Provision, Activate, Suspend, Resume, Decommission, Change.");
    }
}
