using MediatR;
using Obss.Rating.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Commands.CreatePromotion;

public sealed record CreatePromotionCommand(
    string Name,
    string? Description,
    string PromotionType,
    decimal DiscountValue,
    string Currency,
    int? MinQuantity,
    int? MaxQuantity,
    DateTime ValidFrom,
    DateTime? ValidTo,
    bool IsStackable,
    int Priority,
    string? Code,
    int? MaxRedemptions,
    List<CreatePromotionRuleDto> Rules) : IRequest<Result<PromotionDto>>;

public sealed record CreatePromotionRuleDto(
    string RuleType,
    string Operator,
    string Value,
    string Logic);
