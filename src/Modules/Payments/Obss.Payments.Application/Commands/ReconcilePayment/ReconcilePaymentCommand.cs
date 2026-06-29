using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Commands.ReconcilePayment;

public sealed record ReconcilePaymentCommand(
    Guid ReconciliationId,
    Guid ItemId,
    Guid MatchedInvoiceId,
    Guid MatchedPaymentId) : IRequest<Result>;
