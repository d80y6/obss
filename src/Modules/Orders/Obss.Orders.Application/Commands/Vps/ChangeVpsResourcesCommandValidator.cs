using FluentValidation;

namespace Obss.Orders.Application.Commands.Vps;

public sealed class ChangeVpsResourcesCommandValidator : AbstractValidator<ChangeVpsResourcesCommand>
{
    public ChangeVpsResourcesCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.NewCpuCores).InclusiveBetween(1, 128).When(x => x.NewCpuCores.HasValue);
        RuleFor(x => x.NewRamMb).InclusiveBetween(128, 524288).When(x => x.NewRamMb.HasValue);
        RuleFor(x => x.NewStorageGb).InclusiveBetween(5, 50000).When(x => x.NewStorageGb.HasValue);
        RuleFor(x => x.NewBandwidthMbps).InclusiveBetween(1, 100000).When(x => x.NewBandwidthMbps.HasValue);
    }
}
