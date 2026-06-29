using MediatR;
using Obss.Rating.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Commands.CreateRatingRule;

public sealed record CreateRatingRuleCommand(
    string Name,
    string? Description,
    string RuleType,
    Guid? ProductId,
    Guid? OfferId,
    int Priority,
    List<CreateRatingTierDto> Tiers) : IRequest<Result<RatingRuleDto>>;

public sealed record CreateRatingTierDto(
    int FromUnit,
    int? ToUnit,
    decimal Rate,
    string? Description);
