namespace Obss.Payments.Application.DTOs;

public sealed record PaymentReconciliationDto(
    Guid Id,
    string TenantId,
    DateTime ImportDate,
    string ImportSource,
    string? ImportFileName,
    string Status,
    decimal TotalImportAmount,
    decimal TotalReconciledAmount,
    string Currency,
    string ImportedBy,
    DateTime CreatedAt,
    List<ReconciliationItemDto> Items);
