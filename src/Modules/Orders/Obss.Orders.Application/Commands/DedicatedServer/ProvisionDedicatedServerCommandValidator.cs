using FluentValidation;

namespace Obss.Orders.Application.Commands.DedicatedServer;

public sealed class ProvisionDedicatedServerCommandValidator : AbstractValidator<ProvisionDedicatedServerCommand>
{
    public ProvisionDedicatedServerCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Hostname).NotEmpty().MaximumLength(255);
        RuleFor(x => x.CpuCores).InclusiveBetween(1, 256);
        RuleFor(x => x.RamGb).InclusiveBetween(1, 8192);
        RuleFor(x => x.StorageGb).InclusiveBetween(10, 500000);
        RuleFor(x => x.StorageType).NotEmpty().Must(s => s is "SSD" or "HDD" or "NVMe");
        RuleFor(x => x.BandwidthMbps).InclusiveBetween(1, 1000000);
        RuleFor(x => x.OperatingSystem).NotEmpty().MaximumLength(100);
    }
}
