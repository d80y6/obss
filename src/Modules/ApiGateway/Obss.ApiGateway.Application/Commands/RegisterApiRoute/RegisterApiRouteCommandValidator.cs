using FluentValidation;

namespace Obss.ApiGateway.Application.Commands.RegisterApiRoute;

public sealed class RegisterApiRouteCommandValidator : AbstractValidator<RegisterApiRouteCommand>
{
    public RegisterApiRouteCommandValidator()
    {
        RuleFor(x => x.Path)
            .NotEmpty().WithMessage("Path is required.")
            .Must(p => p.StartsWith('/')).WithMessage("Path must start with '/'.");

        RuleFor(x => x.Method)
            .NotEmpty().WithMessage("Method is required.")
            .Must(m => new[] { "GET", "POST", "PUT", "PATCH", "DELETE", "ANY" }.Contains(m.ToUpper()))
            .WithMessage("Method must be one of: GET, POST, PUT, PATCH, DELETE, ANY.");

        RuleFor(x => x.TargetModule)
            .NotEmpty().WithMessage("Target module is required.");

        RuleFor(x => x.TargetPath)
            .NotEmpty().WithMessage("Target path is required.");

        RuleFor(x => x.RateLimitPerMinute)
            .GreaterThan(0).WithMessage("Rate limit must be greater than 0.")
            .LessThanOrEqualTo(10000).WithMessage("Rate limit must not exceed 10000.");
    }
}
