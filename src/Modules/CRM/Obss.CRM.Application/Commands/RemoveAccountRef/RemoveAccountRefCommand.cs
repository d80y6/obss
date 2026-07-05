using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.RemoveAccountRef;

public sealed record RemoveAccountRefCommand(Guid CustomerId, Guid BillingAccountId) : IRequest<Result>;
