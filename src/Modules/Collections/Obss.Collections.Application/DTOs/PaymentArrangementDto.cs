namespace Obss.Collections.Application.DTOs;

public sealed record PaymentArrangementDto(
    Guid Id,
    Guid CollectionCaseId,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    decimal PaidAmount,
    int InstallmentCount,
    decimal InstallmentAmount,
    string Frequency,
    DateTime FirstPaymentDate,
    DateTime? LastPaymentDate,
    DateTime CreatedAt,
    DateTime? DefaultedAt);
