using FluentValidation;

namespace Obss.Collections.Application.Commands.DeleteDunningPolicy;

public sealed class DeleteDunningPolicyCommandValidator : AbstractValidator<DeleteDunningPolicyCommand>
{
    public DeleteDunningPolicyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Policy ID is required.");
    }
}
