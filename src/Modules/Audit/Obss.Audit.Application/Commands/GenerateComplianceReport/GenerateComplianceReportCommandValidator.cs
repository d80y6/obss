using FluentValidation;

namespace Obss.Audit.Application.Commands.GenerateComplianceReport;

internal sealed class GenerateComplianceReportCommandValidator : AbstractValidator<GenerateComplianceReportCommand>
{
    public GenerateComplianceReportCommandValidator()
    {
    }
}
