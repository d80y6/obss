using FluentValidation;

namespace Obss.Orders.Application.Commands.HomeWireless;

public sealed class ActivateHomeWirelessCommandValidator : AbstractValidator<ActivateHomeWirelessCommand>
{
    public ActivateHomeWirelessCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.CpeSerialNumber)
            .NotEmpty().WithMessage("CPE serial number is required.")
            .MaximumLength(100).WithMessage("CPE serial number must not exceed 100 characters.");

        RuleFor(x => x.CpeImei)
            .NotEmpty().WithMessage("CPE IMEI is required.")
            .Length(15).WithMessage("CPE IMEI must be 15 characters.");

        RuleFor(x => x.Iccid)
            .NotEmpty().WithMessage("ICCID is required.")
            .Length(19, 20).WithMessage("ICCID must be 19-20 characters.");

        RuleFor(x => x.Imsi)
            .NotEmpty().WithMessage("IMSI is required.")
            .Length(15).WithMessage("IMSI must be 15 characters.");

        RuleFor(x => x.Msisdn)
            .NotEmpty().WithMessage("MSISDN is required.")
            .MaximumLength(15).WithMessage("MSISDN must not exceed 15 characters.");

        RuleFor(x => x.ApnName)
            .NotEmpty().WithMessage("APN name is required.")
            .MaximumLength(100).WithMessage("APN name must not exceed 100 characters.");

        RuleFor(x => x.QosProfile)
            .NotEmpty().WithMessage("QoS profile is required.")
            .MaximumLength(50).WithMessage("QoS profile must not exceed 50 characters.");
    }
}
