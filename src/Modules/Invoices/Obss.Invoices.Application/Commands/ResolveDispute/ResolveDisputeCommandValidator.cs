using FluentValidation;

namespace Obss.Invoices.Application.Commands.ResolveDispute;

internal sealed class ResolveDisputeCommandValidator : AbstractValidator<ResolveDisputeCommand>
{
    public ResolveDisputeCommandValidator()
    {
        RuleFor(x => x.DisputeId)
            .NotEmpty().WithMessage("Dispute ID is required.");

        RuleFor(x => x.Resolution)
            .NotEmpty().WithMessage("Resolution is required.")
            .MaximumLength(2000).WithMessage("Resolution must not exceed 2000 characters.");

        RuleFor(x => x.ResolvedBy)
            .NotEmpty().WithMessage("Resolved by is required.");
    }
}
