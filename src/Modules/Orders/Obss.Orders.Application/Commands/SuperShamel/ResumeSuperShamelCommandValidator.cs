using FluentValidation;

namespace Obss.Orders.Application.Commands.SuperShamel;

public sealed class ResumeSuperShamelCommandValidator : AbstractValidator<ResumeSuperShamelCommand>
{
    public ResumeSuperShamelCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
    }
}
