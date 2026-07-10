using FluentValidation;

namespace Obss.ServiceQualification.Application.Commands.CheckServiceQualification;

public sealed class CheckServiceQualificationCommandValidator : AbstractValidator<CheckServiceQualificationCommand>
{
    public CheckServiceQualificationCommandValidator()
    {
    }
}
