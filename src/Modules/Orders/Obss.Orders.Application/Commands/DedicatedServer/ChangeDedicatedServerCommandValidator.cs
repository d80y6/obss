using FluentValidation;

namespace Obss.Orders.Application.Commands.DedicatedServer;

public sealed class ChangeDedicatedServerCommandValidator : AbstractValidator<ChangeDedicatedServerCommand>
{
    public ChangeDedicatedServerCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.NewCpuCores).InclusiveBetween(1, 256).When(x => x.NewCpuCores.HasValue);
        RuleFor(x => x.NewRamGb).InclusiveBetween(1, 8192).When(x => x.NewRamGb.HasValue);
        RuleFor(x => x.NewStorageGb).InclusiveBetween(10, 500000).When(x => x.NewStorageGb.HasValue);
        RuleFor(x => x.NewStorageType).Must(s => s is "SSD" or "HDD" or "NVMe").When(x => x.NewStorageType is not null);
        RuleFor(x => x.NewBandwidthMbps).InclusiveBetween(1, 1000000).When(x => x.NewBandwidthMbps.HasValue);
    }
}
