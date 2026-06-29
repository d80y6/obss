namespace Obss.Payments.Application.DTOs;

public sealed record ReconciliationItemDto(
    Guid Id,
    Guid ReconciliationId,
    string ExternalReference,
    decimal Amount,
    string Currency,
    DateTime TransactionDate,
    string? Description,
    Guid? MatchedInvoiceId,
    Guid? MatchedPaymentId,
    string Status,
    string? DiscrepancyReason,
    DateTime CreatedAt);

public sealed record ImportBankStatementRequest(
    string ImportSource,
    string? ImportFileName,
    string Currency,
    List<BankTransactionLine> Transactions);

public sealed record BankTransactionLine(
    string ExternalReference,
    decimal Amount,
    DateTime TransactionDate,
    string? Description);

public sealed record ReconcilePaymentRequest(
    Guid ReconciliationId,
    Guid ItemId,
    Guid MatchedInvoiceId,
    Guid MatchedPaymentId);

public sealed record AutoReconcileRequest(Guid ReconciliationId);
