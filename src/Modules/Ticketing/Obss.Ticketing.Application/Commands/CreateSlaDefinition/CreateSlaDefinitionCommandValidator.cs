using FluentValidation;

namespace Obss.Ticketing.Application.Commands.CreateSlaDefinition;

public sealed class CreateSlaDefinitionCommandValidator : AbstractValidator<CreateSlaDefinitionCommand>
{
    public CreateSlaDefinitionCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.")
            .MaximumLength(100).WithMessage("Tenant ID must not exceed 100 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.ResponseTimeHours)
            .GreaterThan(0).WithMessage("Response time must be greater than 0.");

        RuleFor(x => x.ResolutionTimeHours)
            .GreaterThan(0).WithMessage("Resolution time must be greater than 0.");

        RuleFor(x => x.EscalationTimeHours)
            .GreaterThan(0).WithMessage("Escalation time must be greater than 0.");
    }
}
