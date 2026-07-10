using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.AddBillingAccountRelatedParty;

public sealed record AddBillingAccountRelatedPartyCommand(
    Guid BillingAccountId,
    string PartyId,
    string PartyName,
    string Role) : IRequest<Result<BillingAccountDto>>;
