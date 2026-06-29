using FluentValidation;

namespace Obss.NetworkInventory.Application.Commands.CreateConnectivityLink;

public sealed class CreateConnectivityLinkCommandValidator : AbstractValidator<CreateConnectivityLinkCommand>
{
    public CreateConnectivityLinkCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.SourceElementId)
            .NotEmpty().WithMessage("Source element is required.");

        RuleFor(x => x.SourceInterfaceId)
            .NotEmpty().WithMessage("Source interface is required.");

        RuleFor(x => x.TargetElementId)
            .NotEmpty().WithMessage("Target element is required.");

        RuleFor(x => x.TargetInterfaceId)
            .NotEmpty().WithMessage("Target interface is required.");

        RuleFor(x => x.LinkType)
            .NotEmpty().WithMessage("Link type is required.")
            .Must(v => Enum.TryParse<Domain.ValueObjects.LinkType>(v, out _))
            .WithMessage("Invalid link type.");

        RuleFor(x => x.Bandwidth)
            .GreaterThan(0).WithMessage("Bandwidth must be greater than 0.");

        RuleFor(x => x.LatencyMs)
            .GreaterThanOrEqualTo(0).WithMessage("Latency must be non-negative.");

        RuleFor(x => x.MTU)
            .GreaterThan(0).WithMessage("MTU must be greater than 0.");
    }
}
