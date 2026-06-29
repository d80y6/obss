using FluentValidation;

namespace Obss.CRM.Application.Commands.CreateSegment;

public sealed class CreateSegmentCommandValidator : AbstractValidator<CreateSegmentCommand>
{
    public CreateSegmentCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Segment name is required.")
            .MaximumLength(200).WithMessage("Segment name must not exceed 200 characters.");

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0).WithMessage("Priority must be zero or greater.");

        RuleFor(x => x.Criteria)
            .NotEmpty().WithMessage("Criteria is required.");
    }
}
