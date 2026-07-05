using FluentValidation;

namespace Obss.NetworkInventory.Application.Commands.CreateVLAN;

internal sealed class CreateVLANCommandValidator : AbstractValidator<CreateVLANCommand>
{
    public CreateVLANCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.VLANId)
            .InclusiveBetween(1, 4094).WithMessage("VLAN ID must be between 1 and 4094.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
