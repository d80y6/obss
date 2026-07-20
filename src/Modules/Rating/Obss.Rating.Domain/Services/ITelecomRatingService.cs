using Obss.Rating.Domain.ValueObjects;

namespace Obss.Rating.Domain.Services;

public sealed record BundleAllowance(
    int IncludedMinutes,
    long IncludedDataBytes,
    int IncludedSms);

public sealed record TelecomRatingResult(
    decimal Charge,
    string Currency,
    int? MinutesUsed,
    long? DataBytesUsed,
    int? SmsCount,
    int? IncludedMinutes,
    long? IncludedDataBytes,
    int? IncludedSms,
    decimal? OverageCharge,
    string? DiscountApplied,
    string? Description);

public sealed record DiscountRule(
    string Name,
    decimal Percentage,
    decimal? MaxDiscount,
    bool IsStackable);

public interface ITelecomRatingService
{
    TelecomRatingResult RateDataUsage(long bytesUsed, RatingPlan plan);
    TelecomRatingResult RateVoiceCall(int minutes, DateTime callTime, RatingPlan plan);
    TelecomRatingResult RateSms(int count, RatingPlan plan);
    TelecomRatingResult RateWithBundle(int usedMinutes, long usedDataBytes, int usedSms, BundleAllowance bundle, RatingPlan plan, DateTime? callTime = null);
    TelecomRatingResult ApplyDiscount(TelecomRatingResult baseCharge, DiscountRule discount);
}
