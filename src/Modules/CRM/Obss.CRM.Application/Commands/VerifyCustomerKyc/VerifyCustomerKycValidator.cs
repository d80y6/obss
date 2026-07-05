using FluentValidation;

namespace Obss.CRM.Application.Commands.VerifyCustomerKyc;

internal sealed class VerifyCustomerKycValidator : AbstractValidator<VerifyCustomerKycCommand>
{
    public VerifyCustomerKycValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.VerifiedBy).NotEmpty().MaximumLength(100);
    }
}
