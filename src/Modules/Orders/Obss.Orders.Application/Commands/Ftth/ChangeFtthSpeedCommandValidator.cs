using FluentValidation;

namespace Obss.Orders.Application.Commands.Ftth;

public sealed class ChangeFtthSpeedCommandValidator : AbstractValidator<ChangeFtthSpeedCommand>
{
    public ChangeFtthSpeedCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.OrderItemId)
            .NotEmpty().WithMessage("Order item ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.NewDownloadSpeedMbps)
            .InclusiveBetween(10, 10000).WithMessage("Download speed must be between 10 and 10,000 Mbps.");

        RuleFor(x => x.NewUploadSpeedMbps)
            .InclusiveBetween(10, 10000).WithMessage("Upload speed must be between 10 and 10,000 Mbps.");
    }
}
