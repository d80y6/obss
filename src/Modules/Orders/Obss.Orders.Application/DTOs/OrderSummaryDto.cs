namespace Obss.Orders.Application.DTOs;

public sealed record OrderSummaryDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string CustomerName,
    DateTime OrderDate,
    string Status,
    string OrderType,
    decimal GrandTotal,
    string Currency,
    string? Notes,
    string? Description,
    string? Channel,
    string? Priority,
    DateTime? RequestedStartDate,
    DateTime? RequestedCompletionDate);
