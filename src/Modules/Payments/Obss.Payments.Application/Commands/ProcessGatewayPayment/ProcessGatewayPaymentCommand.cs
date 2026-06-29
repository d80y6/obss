using MediatR;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Commands.ProcessGatewayPayment;

public sealed record ProcessGatewayPaymentCommand(
    decimal Amount,
    string Currency,
    string PaymentMethod,
    string? ReturnUrl,
    string? CancelUrl,
    Guid CustomerId,
    string? Description) : IRequest<Result<PaymentDto>>;
