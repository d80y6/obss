using FluentValidation;

namespace Obss.CRM.Application.Commands.RemoveCustomerFromSegment;

internal sealed class RemoveCustomerFromSegmentCommandValidator : AbstractValidator<RemoveCustomerFromSegmentCommand>
{
    public RemoveCustomerFromSegmentCommandValidator()
    {
        RuleFor(x => x.SegmentId)
            .NotEmpty().WithMessage("Segment ID is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");
    }
}
