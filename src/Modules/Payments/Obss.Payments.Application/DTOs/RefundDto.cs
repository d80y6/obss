namespace Obss.Payments.Application.DTOs;

public sealed record RefundDto(Guid Id, Guid PaymentId, decimal Amount, string Reason, string Status, string? ExternalId, DateTime CreatedAt, DateTime? CompletedAt);
