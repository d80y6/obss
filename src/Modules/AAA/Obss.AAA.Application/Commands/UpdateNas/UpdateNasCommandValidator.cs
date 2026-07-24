using FluentValidation;

namespace Obss.AAA.Application.Commands.UpdateNas;

public sealed class UpdateNasCommandValidator : AbstractValidator<UpdateNasCommand>
{
    public UpdateNasCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NasIpAddress).NotEmpty().MaximumLength(45);
        RuleFor(x => x.NasSecret).NotEmpty().MinimumLength(8).MaximumLength(500);
        RuleFor(x => x.NasType).IsInEnum();
        RuleFor(x => x.Location).MaximumLength(500);
    }
}
