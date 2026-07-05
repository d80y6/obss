using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.DeleteContact;

public sealed record DeleteContactCommand(Guid CustomerId, Guid ContactId) : IRequest<Result>;
