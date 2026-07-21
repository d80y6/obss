using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.OCS.Application.Commands.ReserveCredit;

public sealed record ReserveCreditCommand(
    Guid SubscriptionId,
    decimal Amount,
    string Currency) : IRequest<Result<ReserveCreditResult>>;

public sealed record ReserveCreditResult(
    bool Success,
    Guid? ReservationId,
    decimal ReservedAmount,
    decimal RemainingBalance,
    string? ErrorMessage,
    IReadOnlyList<Guid> CreditPoolIds);
