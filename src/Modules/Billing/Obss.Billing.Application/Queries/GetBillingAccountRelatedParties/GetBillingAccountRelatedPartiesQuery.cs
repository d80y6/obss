using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetBillingAccountRelatedParties;

public sealed record GetBillingAccountRelatedPartiesQuery(
    Guid BillingAccountId) : IRequest<Result<List<RelatedPartyDto>>>;
