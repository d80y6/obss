using FluentValidation;

namespace Obss.Reporting.Application.Commands.CreateReportDefinition;

public sealed class CreateReportDefinitionCommandValidator : AbstractValidator<CreateReportDefinitionCommand>
{
    public CreateReportDefinitionCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.ReportType)
            .NotEmpty().WithMessage("Report type is required.");

        RuleFor(x => x.DataSource)
            .NotEmpty().WithMessage("Data source is required.")
            .MaximumLength(200).WithMessage("Data source must not exceed 200 characters.");

        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Query is required.");

        RuleFor(x => x.OutputFormat)
            .NotEmpty().WithMessage("Output format is required.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Schedule)
            .MaximumLength(100).WithMessage("Schedule must not exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Schedule));
    }
}
