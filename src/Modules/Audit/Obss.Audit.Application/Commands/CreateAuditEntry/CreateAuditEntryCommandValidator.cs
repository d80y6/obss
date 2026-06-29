using FluentValidation;

namespace Obss.Audit.Application.Commands.CreateAuditEntry;

public sealed class CreateAuditEntryCommandValidator : AbstractValidator<CreateAuditEntryCommand>
{
    public CreateAuditEntryCommandValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("Entity type is required.")
            .MaximumLength(100).WithMessage("Entity type must not exceed 100 characters.");

        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("Entity ID is required.")
            .MaximumLength(200).WithMessage("Entity ID must not exceed 200 characters.");

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required.")
            .MaximumLength(50).WithMessage("Action must not exceed 50 characters.");

        RuleFor(x => x.IpAddress)
            .MaximumLength(50).WithMessage("IP address must not exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.IpAddress));

        RuleFor(x => x.UserAgent)
            .MaximumLength(500).WithMessage("User agent must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.UserAgent));

        RuleFor(x => x.PerformedById)
            .MaximumLength(200).WithMessage("Performed by ID must not exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.PerformedById));

        RuleFor(x => x.CorrelationId)
            .MaximumLength(200).WithMessage("Correlation ID must not exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.CorrelationId));
    }
}
