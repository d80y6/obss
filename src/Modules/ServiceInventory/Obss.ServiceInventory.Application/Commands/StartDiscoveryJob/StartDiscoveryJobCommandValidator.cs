using FluentValidation;

namespace Obss.ServiceInventory.Application.Commands.StartDiscoveryJob;

internal sealed class StartDiscoveryJobCommandValidator : AbstractValidator<StartDiscoveryJobCommand>
{
    public StartDiscoveryJobCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.DiscoveryType)
            .NotEmpty().WithMessage("Discovery type is required.")
            .MaximumLength(100).WithMessage("Discovery type must not exceed 100 characters.");

        RuleFor(x => x.Configuration)
            .MaximumLength(4000).WithMessage("Configuration must not exceed 4000 characters.")
            .When(x => x.Configuration is not null);

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("Created by is required.")
            .MaximumLength(100).WithMessage("Created by must not exceed 100 characters.");
    }
}
