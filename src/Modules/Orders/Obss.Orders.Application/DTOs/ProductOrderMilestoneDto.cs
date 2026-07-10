namespace Obss.Orders.Application.DTOs;

public sealed record ProductOrderMilestoneDto(
    Guid Id,
    Guid ProductOrderId,
    string Name,
    string Description,
    DateTime MilestoneDate,
    string Status);
