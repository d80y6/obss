using FluentValidation;

namespace Obss.ServiceInventory.Application.Commands.RemoveTopologyLink;

internal sealed class RemoveTopologyLinkCommandValidator : AbstractValidator<RemoveTopologyLinkCommand>
{
    public RemoveTopologyLinkCommandValidator()
    {
        RuleFor(x => x.ServiceTopologyId)
            .NotEmpty().WithMessage("Service topology ID is required.");
        RuleFor(x => x.LinkId)
            .NotEmpty().WithMessage("Link ID is required.");
    }
}
