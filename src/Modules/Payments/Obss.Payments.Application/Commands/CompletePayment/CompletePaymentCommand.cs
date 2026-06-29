using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Commands.CompletePayment;

public sealed record CompletePaymentCommand(Guid PaymentId) : IRequest<Result>;
