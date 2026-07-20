using FluentValidation;

namespace Obss.Orders.Application.Commands.SuperShamel;

public sealed class SubscribeSuperShamelCommandValidator : AbstractValidator<SubscribeSuperShamelCommand>
{
    public SubscribeSuperShamelCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.FtthSpeedMbps).Must(x => x is 50 or 100);
        RuleFor(x => x.FtthOntSerial).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FtthLoid).NotEmpty().MaximumLength(100);
        RuleFor(x => x.HatifTawasolTelephoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.HatifTawasolPackage).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Yemen4GIccid).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Yemen4GImsi).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Yemen4GMsisdn).NotEmpty().MaximumLength(20);
        RuleFor(x => x.InstallationAddress).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ContractMonths).Must(x => x is 12 or 24);
    }
}
