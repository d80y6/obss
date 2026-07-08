namespace Obss.Provisioning.Application.DTOs;

public sealed record ServiceOrderDto(
    Guid Id,
    string State,
    string? ExternalId,
    string? Priority,
    string? Description,
    string? Category,
    DateTime? RequestedStartDate,
    DateTime? RequestedCompletionDate,
    DateTime OrderDate,
    DateTime? StatusChangeDate,
    string? CompletionMessage,
    List<ServiceOrderItemDto> Items,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record ServiceOrderItemDto(
    Guid Id,
    Guid ServiceOrderId,
    Guid? ServiceId,
    string Action,
    int Quantity,
    string? Description,
    string State,
    DateTime? RequestedStartDate,
    DateTime? RequestedCompletionDate,
    DateTime? CompletedDate,
    string? ErrorMessage);
