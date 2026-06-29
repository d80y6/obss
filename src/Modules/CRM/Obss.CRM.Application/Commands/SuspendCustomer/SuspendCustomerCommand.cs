using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.SuspendCustomer;

public sealed record SuspendCustomerCommand(Guid CustomerId, string Reason) : IRequest<Result>;
