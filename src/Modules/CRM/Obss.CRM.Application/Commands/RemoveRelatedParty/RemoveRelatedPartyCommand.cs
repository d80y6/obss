using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.RemoveRelatedParty;

public sealed record RemoveRelatedPartyCommand(Guid CustomerId, Guid ReferredId) : IRequest<Result>;
