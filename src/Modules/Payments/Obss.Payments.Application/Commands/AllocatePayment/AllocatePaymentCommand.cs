using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Commands.AllocatePayment;

public sealed record AllocatePaymentCommand(
    Guid PaymentId,
    Guid InvoiceId,
    decimal Amount) : IRequest<Result>;
