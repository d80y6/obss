using FluentValidation;
using Obss.Orders.Domain.ValueObjects;

namespace Obss.Orders.Application.Commands.AddItemRelationship;

public sealed class AddItemRelationshipCommandValidator : AbstractValidator<AddItemRelationshipCommand>
{
    public AddItemRelationshipCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.TargetItemId).NotEmpty();
        RuleFor(x => x.Type).NotEmpty().Must(t => Enum.TryParse<RelationshipType>(t, true, out _));
    }
}
