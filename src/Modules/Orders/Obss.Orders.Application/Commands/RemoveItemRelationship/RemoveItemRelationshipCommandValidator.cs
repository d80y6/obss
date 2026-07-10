using FluentValidation;

namespace Obss.Orders.Application.Commands.RemoveItemRelationship;

public sealed class RemoveItemRelationshipCommandValidator : AbstractValidator<RemoveItemRelationshipCommand>
{
    public RemoveItemRelationshipCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.RelationshipId).NotEmpty();
    }
}
