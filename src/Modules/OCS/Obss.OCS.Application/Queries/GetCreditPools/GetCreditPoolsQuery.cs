using MediatR;
using Obss.OCS.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.OCS.Application.Queries.GetCreditPools;

public sealed record GetCreditPoolsQuery(Guid SubscriptionId) : IRequest<Result<IReadOnlyList<CreditPoolDto>>>;
