using FluentValidation;

namespace Obss.ServiceInventory.Application.Commands.UpdateService;

internal sealed class UpdateServiceCommandValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceCommandValidator()
    {
        RuleFor(x => x.ServiceId)
            .NotEmpty().WithMessage("Service ID is required.");
    }
}
