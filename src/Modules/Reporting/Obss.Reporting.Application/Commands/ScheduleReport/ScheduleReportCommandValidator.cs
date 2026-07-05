using FluentValidation;

namespace Obss.Reporting.Application.Commands.ScheduleReport;

internal sealed class ScheduleReportCommandValidator : AbstractValidator<ScheduleReportCommand>
{
    public ScheduleReportCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.ReportDefinitionId)
            .NotEmpty().WithMessage("Report definition ID is required.");

        RuleFor(x => x.CronExpression)
            .NotEmpty().WithMessage("Cron expression is required.")
            .MaximumLength(100).WithMessage("Cron expression must not exceed 100 characters.");

        RuleFor(x => x.Recipients)
            .NotNull().WithMessage("Recipients list is required.")
            .NotEmpty().WithMessage("At least one recipient is required.");
    }
}
