using FluentValidation;

namespace Obss.Audit.Application.Commands.CreateAuditPolicy;

internal sealed class CreateAuditPolicyCommandValidator : AbstractValidator<CreateAuditPolicyCommand>
{
    public CreateAuditPolicyCommandValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("Entity type is required.")
            .MaximumLength(200).WithMessage("Entity type must not exceed 200 characters.");

        RuleFor(x => x.RetentionDays)
            .GreaterThan(0).WithMessage("Retention days must be greater than 0.");
    }
}
