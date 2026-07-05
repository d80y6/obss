using FluentValidation;

namespace Obss.NetworkInventory.Application.Commands.CreateSubnet;

internal sealed class CreateSubnetCommandValidator : AbstractValidator<CreateSubnetCommand>
{
    public CreateSubnetCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.Network)
            .NotEmpty().WithMessage("Network is required.")
            .MaximumLength(45).WithMessage("Network must not exceed 45 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.VLANId)
            .InclusiveBetween(1, 4094).WithMessage("VLAN ID must be between 1 and 4094.");
    }
}
