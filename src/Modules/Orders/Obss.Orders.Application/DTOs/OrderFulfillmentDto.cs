namespace Obss.Orders.Application.DTOs;

public sealed record OrderFulfillmentDto(
    Guid Id,
    Guid OrderId,
    string Status,
    Guid? WorkflowInstanceId,
    DateTime StartedAt,
    DateTime? CompletedAt,
    string? ErrorMessage);
