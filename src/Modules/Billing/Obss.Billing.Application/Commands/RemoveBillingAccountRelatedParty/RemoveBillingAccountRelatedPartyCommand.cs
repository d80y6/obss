using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.RemoveBillingAccountRelatedParty;

public sealed record RemoveBillingAccountRelatedPartyCommand(
    Guid BillingAccountId,
    string PartyId) : IRequest<Result>;
