using FluentValidation;

namespace Obss.CRM.Application.Commands.DeleteContact;

internal sealed class DeleteContactCommandValidator : AbstractValidator<DeleteContactCommand>
{
    public DeleteContactCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");
        RuleFor(x => x.ContactId)
            .NotEmpty().WithMessage("Contact ID is required.");
    }
}
