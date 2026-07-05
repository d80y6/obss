using FluentValidation;

namespace Obss.ServiceInventory.Application.Commands.AddTopologyLink;

internal sealed class AddTopologyLinkCommandValidator : AbstractValidator<AddTopologyLinkCommand>
{
    public AddTopologyLinkCommandValidator()
    {
        RuleFor(x => x.ServiceTopologyId)
            .NotEmpty().WithMessage("Service topology ID is required.");

        RuleFor(x => x.SourceServiceId)
            .NotEmpty().WithMessage("Source service ID is required.");

        RuleFor(x => x.TargetServiceId)
            .NotEmpty().WithMessage("Target service ID is required.");

        RuleFor(x => x.LinkType)
            .NotEmpty().WithMessage("Link type is required.")
            .MaximumLength(50).WithMessage("Link type must not exceed 50 characters.");

        RuleFor(x => x.Direction)
            .NotEmpty().WithMessage("Direction is required.")
            .MaximumLength(20).WithMessage("Direction must not exceed 20 characters.");
    }
}
