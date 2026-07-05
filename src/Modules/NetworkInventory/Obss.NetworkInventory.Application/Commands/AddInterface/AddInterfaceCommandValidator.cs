using FluentValidation;

namespace Obss.NetworkInventory.Application.Commands.AddInterface;

internal sealed class AddInterfaceCommandValidator : AbstractValidator<AddInterfaceCommand>
{
    public AddInterfaceCommandValidator()
    {
        RuleFor(x => x.NetworkElementId)
            .NotEmpty().WithMessage("Network element ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.InterfaceType)
            .NotEmpty().WithMessage("Interface type is required.")
            .MaximumLength(100).WithMessage("Interface type must not exceed 100 characters.");

        RuleFor(x => x.Speed)
            .GreaterThan(0).WithMessage("Speed must be greater than zero.");

        RuleFor(x => x.MTU)
            .GreaterThan(0).WithMessage("MTU must be greater than zero.")
            .LessThanOrEqualTo(9000).WithMessage("MTU must not exceed 9000.");

        RuleFor(x => x.MacAddress)
            .MaximumLength(17).WithMessage("MAC address must not exceed 17 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.MacAddress));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
