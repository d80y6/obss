using FluentValidation;

namespace Obss.NetworkInventory.Application.Commands.AllocateIP;

internal sealed class AllocateIPCommandValidator : AbstractValidator<AllocateIPCommand>
{
    public AllocateIPCommandValidator()
    {
        RuleFor(x => x.NetworkElementId)
            .NotEmpty().WithMessage("Network element ID is required.");

        RuleFor(x => x.IPAddress)
            .NotEmpty().WithMessage("IP address is required.")
            .MaximumLength(45).WithMessage("IP address must not exceed 45 characters.");

        RuleFor(x => x.SubnetMask)
            .NotEmpty().WithMessage("Subnet mask is required.")
            .MaximumLength(45).WithMessage("Subnet mask must not exceed 45 characters.");

        RuleFor(x => x.AddressType)
            .NotEmpty().WithMessage("Address type is required.")
            .MaximumLength(50).WithMessage("Address type must not exceed 50 characters.");

        RuleFor(x => x.Gateway)
            .MaximumLength(45).WithMessage("Gateway must not exceed 45 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Gateway));

        RuleFor(x => x.AssignedTo)
            .MaximumLength(200).WithMessage("Assigned to must not exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.AssignedTo));
    }
}
