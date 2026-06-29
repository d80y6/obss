using FluentValidation;

namespace Obss.ApiGateway.Application.Commands.CreateApiKey;

public sealed class CreateApiKeyCommandValidator : AbstractValidator<CreateApiKeyCommand>
{
    public CreateApiKeyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.RateLimitPerMinute)
            .GreaterThan(0).WithMessage("Rate limit must be greater than 0.")
            .LessThanOrEqualTo(10000).WithMessage("Rate limit must not exceed 10000.");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future.")
            .When(x => x.ExpiresAt.HasValue);
    }
}
