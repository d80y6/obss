using FluentValidation;

namespace Obss.Invoices.Application.Commands.RejectDispute;

internal sealed class RejectDisputeCommandValidator : AbstractValidator<RejectDisputeCommand>
{
    public RejectDisputeCommandValidator()
    {
        RuleFor(x => x.DisputeId)
            .NotEmpty().WithMessage("Dispute ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
