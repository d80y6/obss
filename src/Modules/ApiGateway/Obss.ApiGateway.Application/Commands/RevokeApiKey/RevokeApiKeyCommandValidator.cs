using FluentValidation;

namespace Obss.ApiGateway.Application.Commands.RevokeApiKey;

internal sealed class RevokeApiKeyCommandValidator : AbstractValidator<RevokeApiKeyCommand>
{
    public RevokeApiKeyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("API key ID is required.");
    }
}
