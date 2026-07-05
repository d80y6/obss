using FluentValidation;

namespace Obss.Payments.Application.Commands.AutoReconcile;

internal sealed class AutoReconcileCommandValidator : AbstractValidator<AutoReconcileCommand>
{
    public AutoReconcileCommandValidator()
    {
        RuleFor(x => x.ReconciliationId)
            .NotEmpty().WithMessage("Reconciliation ID is required.");
    }
}
