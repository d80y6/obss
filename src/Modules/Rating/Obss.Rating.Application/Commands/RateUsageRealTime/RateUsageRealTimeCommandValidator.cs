using FluentValidation;

namespace Obss.Rating.Application.Commands.RateUsageRealTime;

internal sealed class RateUsageRealTimeCommandValidator : AbstractValidator<RateUsageRealTimeCommand>
{
    public RateUsageRealTimeCommandValidator()
    {
        RuleFor(x => x.UsageRecordId)
            .NotEmpty().WithMessage("Usage record ID is required.");
    }
}
