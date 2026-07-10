using FluentValidation;

namespace Obss.Billing.Application.Commands.CreateBillPresentationMedia;

public sealed class CreateBillPresentationMediaCommandValidator : AbstractValidator<CreateBillPresentationMediaCommand>
{
    public CreateBillPresentationMediaCommandValidator()
    {
        RuleFor(x => x.BillingAccountId)
            .NotEmpty().WithMessage("Billing account ID is required.");

        RuleFor(x => x.MediaType)
            .NotEmpty().WithMessage("Media type is required.")
            .Must(m => m is "Email" or "Paper" or "Portal").WithMessage("Media type must be Email, Paper, or Portal.");

        RuleFor(x => x.EmailAddress)
            .EmailAddress().When(x => x.EmailAddress is not null)
            .WithMessage("Invalid email address.");

        RuleFor(x => x.PaperFormat)
            .Must(f => f is "A4" or "Letter").When(x => x.PaperFormat is not null)
            .WithMessage("Paper format must be A4 or Letter.");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required.")
            .MaximumLength(10).WithMessage("Language must not exceed 10 characters.");
    }
}
