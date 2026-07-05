using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.DeleteCustomer;

public sealed record DeleteCustomerCommand(Guid CustomerId) : IRequest<Result>;
