using FluentValidation;

namespace Obss.Collections.Application.Commands.ResolveCollectionCase;

internal sealed class ResolveCollectionCaseCommandValidator : AbstractValidator<ResolveCollectionCaseCommand>
{
    public ResolveCollectionCaseCommandValidator()
    {
        RuleFor(x => x.CaseId)
            .NotEmpty().WithMessage("Case ID is required.");
    }
}
