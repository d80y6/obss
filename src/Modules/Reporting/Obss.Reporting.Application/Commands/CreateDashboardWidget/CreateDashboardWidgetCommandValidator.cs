using FluentValidation;

namespace Obss.Reporting.Application.Commands.CreateDashboardWidget;

public sealed class CreateDashboardWidgetCommandValidator : AbstractValidator<CreateDashboardWidgetCommand>
{
    public CreateDashboardWidgetCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.WidgetType)
            .NotEmpty().WithMessage("Widget type is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Configuration)
            .NotEmpty().WithMessage("Configuration is required.");

        RuleFor(x => x.Position)
            .GreaterThanOrEqualTo(0).WithMessage("Position must be a non-negative number.");

        RuleFor(x => x.Size)
            .NotEmpty().WithMessage("Size is required.");

        RuleFor(x => x.DataSource)
            .NotEmpty().WithMessage("Data source is required.")
            .MaximumLength(200).WithMessage("Data source must not exceed 200 characters.");

        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Query is required.");
    }
}
