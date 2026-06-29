using Mapster;
using Obss.Rating.Application.DTOs;
using Obss.Rating.Domain.DomainServices;
using Obss.Rating.Domain.Entities;

namespace Obss.Rating.Application.Mappings;

public static class RatingMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<UsageRecord, UsageRecordDto>.NewConfig()
            .Map(dest => dest.RecordType, src => src.RecordType.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString());

        TypeAdapterConfig<RatingRule, RatingRuleDto>.NewConfig()
            .Map(dest => dest.RuleType, src => src.RuleType.ToString())
            .Map(dest => dest.Tiers, src => src.Tiers.Adapt<List<RatingTierDto>>());

        TypeAdapterConfig<RatingTier, RatingTierDto>.NewConfig();

        TypeAdapterConfig<RatedUsageResult, RatedUsageDto>.NewConfig();

        TypeAdapterConfig<Promotion, PromotionDto>.NewConfig()
            .Map(dest => dest.PromotionType, src => src.PromotionType.ToString())
            .Map(dest => dest.Rules, src => src.Rules.Adapt<List<PromotionRuleDto>>());

        TypeAdapterConfig<PromotionRule, PromotionRuleDto>.NewConfig()
            .Map(dest => dest.RuleType, src => src.RuleType.ToString())
            .Map(dest => dest.Operator, src => src.Operator.ToString())
            .Map(dest => dest.Logic, src => src.Logic.ToString());
    }
}
