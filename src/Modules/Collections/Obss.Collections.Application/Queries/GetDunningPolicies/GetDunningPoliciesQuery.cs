using MediatR;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Queries.GetDunningPolicies;

public sealed record GetDunningPoliciesQuery(bool? ActiveOnly = null) : IRequest<Result<IReadOnlyList<DunningPolicyDto>>>;
