namespace Obss.Orders.Application.DTOs;

public sealed record ProductOrderItemRelationshipDto(
    Guid Id,
    Guid ProductOrderItemId,
    Guid TargetItemId,
    string Type);
