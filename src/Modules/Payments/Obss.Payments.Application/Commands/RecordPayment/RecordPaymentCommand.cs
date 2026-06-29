using MediatR;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Commands.RecordPayment;

public sealed record RecordPaymentCommand(
    Guid CustomerId,
    decimal Amount,
    string Currency,
    string PaymentMethod,
    string? PaymentReference,
    Guid? InvoiceId,
    string? Notes) : IRequest<Result<PaymentDto>>;
