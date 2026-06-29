using MediatR;
using Obss.Rating.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Queries.GetRules;

public sealed record GetRulesQuery : IRequest<Result<IReadOnlyList<RatingRuleDto>>>;
