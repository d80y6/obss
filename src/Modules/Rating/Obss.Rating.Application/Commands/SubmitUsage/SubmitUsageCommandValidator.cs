using FluentValidation;

namespace Obss.Rating.Application.Commands.SubmitUsage;

public sealed class SubmitUsageCommandValidator : AbstractValidator<SubmitUsageCommand>
{
    public SubmitUsageCommandValidator()
    {
        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("Service ID is required.");

        RuleFor(x => x.RecordType)
            .NotEmpty().WithMessage("Record type is required.")
            .Must(v => v is "Voice" or "Data" or "SMS" or "Session")
            .WithMessage("Record type must be Voice, Data, SMS, or Session.");

        RuleFor(x => x.UsageType)
            .NotEmpty().WithMessage("Usage type is required.")
            .MaximumLength(100).WithMessage("Usage type must not exceed 100 characters.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required.")
            .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time.");

        RuleFor(x => x.Duration)
            .GreaterThanOrEqualTo(0).WithMessage("Duration must be non-negative.");

        RuleFor(x => x.Volume)
            .GreaterThanOrEqualTo(0).WithMessage("Volume must be non-negative.");

        RuleFor(x => x.SourceIdentifier)
            .NotEmpty().WithMessage("Source identifier is required.")
            .MaximumLength(200).WithMessage("Source identifier must not exceed 200 characters.");

        RuleFor(x => x.DestinationIdentifier)
            .NotEmpty().WithMessage("Destination identifier is required.")
            .MaximumLength(200).WithMessage("Destination identifier must not exceed 200 characters.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter code.")
            .Must(v => v is "USD" or "YER")
            .WithMessage("Currency must be USD or YER.");
    }
}
