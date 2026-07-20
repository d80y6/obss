using FluentValidation;

namespace Obss.Orders.Application.Commands.WebHosting;

public sealed class ProvisionWebHostingCommandValidator : AbstractValidator<ProvisionWebHostingCommand>
{
    public ProvisionWebHostingCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.DomainName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.HostingPlan).NotEmpty().Must(p => p is "shared" or "business" or "enterprise");
        RuleFor(x => x.DiskSpaceMb).InclusiveBetween(100, 1000000);
        RuleFor(x => x.BandwidthGb).InclusiveBetween(1, 100000);
        RuleFor(x => x.EmailAccounts).InclusiveBetween(0, 10000);
        RuleFor(x => x.DatabaseCount).InclusiveBetween(0, 1000);
    }
}
