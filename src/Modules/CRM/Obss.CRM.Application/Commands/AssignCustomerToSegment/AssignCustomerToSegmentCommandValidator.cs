using FluentValidation;

namespace Obss.CRM.Application.Commands.AssignCustomerToSegment;

internal sealed class AssignCustomerToSegmentCommandValidator : AbstractValidator<AssignCustomerToSegmentCommand>
{
    public AssignCustomerToSegmentCommandValidator()
    {
        RuleFor(x => x.SegmentId)
            .NotEmpty().WithMessage("Segment ID is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.AssignedBy)
            .NotEmpty().WithMessage("Assigned by is required.");
    }
}
