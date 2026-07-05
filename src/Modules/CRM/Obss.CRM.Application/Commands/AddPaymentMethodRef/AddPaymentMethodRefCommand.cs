using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.AddPaymentMethodRef;

public sealed record AddPaymentMethodRefCommand(
    Guid CustomerId,
    Guid PaymentMethodId,
    string Name,
    string? Href) : IRequest<Result>;
