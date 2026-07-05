using FluentValidation;

namespace Obss.NetworkInventory.Application.Commands.SaveTopologyMap;

internal sealed class SaveTopologyMapCommandValidator : AbstractValidator<SaveTopologyMapCommand>
{
    public SaveTopologyMapCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
    }
}
