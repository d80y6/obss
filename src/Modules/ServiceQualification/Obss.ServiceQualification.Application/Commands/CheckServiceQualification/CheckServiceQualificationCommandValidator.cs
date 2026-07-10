using FluentValidation;

namespace Obss.ServiceQualification.Application.Commands.CheckServiceQualification;

public sealed class CheckServiceQualificationCommandValidator : AbstractValidator<CheckServiceQualificationCommand>
{
    public CheckServiceQualificationCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Address.Street).NotEmpty().WithMessage("Street is required.");
        RuleFor(x => x.Address.City).NotEmpty().WithMessage("City is required.");
        RuleFor(x => x.Address.Country).NotEmpty().WithMessage("Country is required.");
        RuleFor(x => x.RequestedServices).NotEmpty().WithMessage("At least one service must be requested.");
        RuleForEach(x => x.RequestedServices).ChildRules(service =>
        {
            service.RuleFor(s => s.ServiceName).NotEmpty().WithMessage("Service name is required.");
        });
    }
}
