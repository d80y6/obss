using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Commands.RefundPayment;

public sealed record RefundPaymentCommand(
    Guid PaymentId,
    decimal Amount,
    string Reason) : IRequest<Result>;
