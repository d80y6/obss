using FluentValidation;

namespace Obss.Reporting.Application.Commands.ExecuteReport;

internal sealed class ExecuteReportCommandValidator : AbstractValidator<ExecuteReportCommand>
{
    public ExecuteReportCommandValidator()
    {
        RuleFor(x => x.ReportDefinitionId)
            .NotEmpty().WithMessage("Report definition ID is required.");

        RuleFor(x => x.ExecutedBy)
            .NotEmpty().WithMessage("Executed by is required.")
            .MaximumLength(100).WithMessage("Executed by must not exceed 100 characters.");
    }
}
