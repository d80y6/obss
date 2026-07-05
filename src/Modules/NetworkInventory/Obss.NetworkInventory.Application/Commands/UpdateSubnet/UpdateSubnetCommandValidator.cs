using FluentValidation;

namespace Obss.NetworkInventory.Application.Commands.UpdateSubnet;

internal sealed class UpdateSubnetCommandValidator : AbstractValidator<UpdateSubnetCommand>
{
    public UpdateSubnetCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Subnet ID is required.");

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
