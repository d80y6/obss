using FluentValidation;

namespace Obss.NetworkInventory.Application.Commands.UpdateNetworkElement;

internal sealed class UpdateNetworkElementCommandValidator : AbstractValidator<UpdateNetworkElementCommand>
{
    public UpdateNetworkElementCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Network element ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Hostname)
            .NotEmpty().WithMessage("Hostname is required.")
            .MaximumLength(255).WithMessage("Hostname must not exceed 255 characters.");

        RuleFor(x => x.Vendor)
            .NotEmpty().WithMessage("Vendor is required.")
            .MaximumLength(100).WithMessage("Vendor must not exceed 100 characters.");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required.")
            .MaximumLength(100).WithMessage("Model must not exceed 100 characters.");

        RuleFor(x => x.SoftwareVersion)
            .MaximumLength(100).WithMessage("Software version must not exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.SoftwareVersion));

        RuleFor(x => x.SerialNumber)
            .MaximumLength(100).WithMessage("Serial number must not exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.SerialNumber));

        RuleFor(x => x.ManagementIP)
            .MaximumLength(45).WithMessage("Management IP must not exceed 45 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.ManagementIP));
    }
}
