using FluentValidation;

namespace Obss.NetworkInventory.Application.Commands.CreateOLT;

public sealed class CreateOLTCommandValidator : AbstractValidator<CreateOLTCommand>
{
    public CreateOLTCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Hostname)
            .NotEmpty().WithMessage("Hostname is required.")
            .MaximumLength(255);

        RuleFor(x => x.IPAddress)
            .NotEmpty().WithMessage("IP address is required.")
            .MaximumLength(45);

        RuleFor(x => x.Vendor)
            .NotEmpty().WithMessage("Vendor is required.")
            .MaximumLength(100);

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required.")
            .MaximumLength(100);

        RuleFor(x => x.MaxPONPorts)
            .InclusiveBetween(1, 128).WithMessage("Max PON ports must be between 1 and 128.");

        RuleFor(x => x.MaxONTPerPort)
            .InclusiveBetween(1, 256).WithMessage("Max ONT per port must be between 1 and 256.");

        RuleFor(x => x.MaxBandwidth)
            .InclusiveBetween(1, 400).WithMessage("Max bandwidth must be between 1 and 400 Gbps.");
    }
}
