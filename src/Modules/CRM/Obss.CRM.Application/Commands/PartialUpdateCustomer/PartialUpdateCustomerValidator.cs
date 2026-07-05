using FluentValidation;

namespace Obss.CRM.Application.Commands.PartialUpdateCustomer;

internal sealed class PartialUpdateCustomerValidator : AbstractValidator<PartialUpdateCustomerCommand>
{
    public PartialUpdateCustomerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.StatusReason).MaximumLength(500);
        RuleFor(x => x.ExternalId).MaximumLength(100);
    }
}
