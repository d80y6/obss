using FluentValidation;

namespace Obss.ServiceInventory.Application.Commands.CreateTopology;

internal sealed class CreateTopologyCommandValidator : AbstractValidator<CreateTopologyCommand>
{
    public CreateTopologyCommandValidator()
    {
        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("Service ID is required.");

        RuleFor(x => x.TopologyType)
            .NotEmpty().WithMessage("Topology type is required.")
            .MaximumLength(50).WithMessage("Topology type must not exceed 50 characters.");
    }
}
