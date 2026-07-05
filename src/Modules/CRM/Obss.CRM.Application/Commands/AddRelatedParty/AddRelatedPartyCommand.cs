using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.AddRelatedParty;

public sealed record AddRelatedPartyCommand(
    Guid CustomerId,
    string Name,
    string Role,
    Guid ReferredId,
    string ReferredType) : IRequest<Result>;
