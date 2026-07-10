using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.DeleteBillingAccount;

public sealed record DeleteBillingAccountCommand(Guid Id) : IRequest<Result>;
