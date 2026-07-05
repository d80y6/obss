using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.RemovePaymentMethodRef;

public sealed record RemovePaymentMethodRefCommand(Guid CustomerId, Guid PaymentMethodId) : IRequest<Result>;
