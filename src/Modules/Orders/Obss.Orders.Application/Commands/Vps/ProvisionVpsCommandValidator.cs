using FluentValidation;

namespace Obss.Orders.Application.Commands.Vps;

public sealed class ProvisionVpsCommandValidator : AbstractValidator<ProvisionVpsCommand>
{
    public ProvisionVpsCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Hostname).NotEmpty().MaximumLength(255);
        RuleFor(x => x.CpuCores).InclusiveBetween(1, 128);
        RuleFor(x => x.RamMb).InclusiveBetween(128, 524288);
        RuleFor(x => x.StorageGb).InclusiveBetween(5, 50000);
        RuleFor(x => x.BandwidthMbps).InclusiveBetween(1, 100000);
        RuleFor(x => x.OperatingSystem).NotEmpty().MaximumLength(100);
        RuleFor(x => x.VirtualizationType).NotEmpty().Must(v => v is "KVM" or "VMware" or "Xen");
        RuleFor(x => x.PublicIpCount).InclusiveBetween(0, 256);
    }
}
