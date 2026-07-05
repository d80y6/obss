using FluentValidation;

namespace Obss.Collections.Application.Commands.SendDunningNotice;

internal sealed class SendDunningNoticeCommandValidator : AbstractValidator<SendDunningNoticeCommand>
{
    public SendDunningNoticeCommandValidator()
    {
        RuleFor(x => x.CaseId)
            .NotEmpty().WithMessage("Case ID is required.");
    }
}
