namespace Obss.Collections.Application.DTOs;

public sealed record CollectionCaseDto(
    Guid Id,
    string TenantId,
    Guid CustomerId,
    string CustomerName,
    string Status,
    decimal TotalOverdueAmount,
    string Currency,
    int CurrentDunningLevel,
    DateTime OpenedAt,
    DateTime? LastActionAt,
    DateTime? ResolvedAt,
    string? AssignedTo,
    string? Notes,
    List<CollectionActionDto> Actions,
    List<PaymentArrangementDto> PaymentArrangements);
