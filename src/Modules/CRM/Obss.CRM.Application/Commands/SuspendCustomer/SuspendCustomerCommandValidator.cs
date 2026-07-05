using FluentValidation;

namespace Obss.CRM.Application.Commands.SuspendCustomer;

internal sealed class SuspendCustomerCommandValidator : AbstractValidator<SuspendCustomerCommand>
{
    public SuspendCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
