using Obss.Rating.Domain.ValueObjects;

namespace Obss.Rating.Domain.Services;

public sealed class TelecomRatingService : ITelecomRatingService
{
    private const string CurrencyYer = "YER";

    public TelecomRatingResult RateDataUsage(long bytesUsed, RatingPlan plan)
    {
        var totalMb = bytesUsed / (1024.0 * 1024.0);
        var chargeableMb = (long)Math.Ceiling(totalMb);

        if (plan.IncludedDataBytes.HasValue && bytesUsed <= plan.IncludedDataBytes.Value)
        {
            return new TelecomRatingResult(
                0, CurrencyYer, null, bytesUsed, null, null, plan.IncludedDataBytes, null, 0, null, "Within data allowance");
        }

        var overageBytes = bytesUsed - (plan.IncludedDataBytes ?? 0);
        var overageMb = (long)Math.Ceiling(overageBytes / (1024.0 * 1024.0));
        var perMb = plan.PerMbRate ?? 0;
        var charge = overageMb * perMb;

        return new TelecomRatingResult(
            charge, CurrencyYer, null, bytesUsed, null, null, plan.IncludedDataBytes, null, charge, null,
            $"{chargeableMb}MB data: {charge} YER");
    }

    public TelecomRatingResult RateVoiceCall(int minutes, DateTime callTime, RatingPlan plan)
    {
        if (plan.IncludedMinutes.HasValue && minutes <= plan.IncludedMinutes.Value)
        {
            return new TelecomRatingResult(
                0, CurrencyYer, minutes, null, null, plan.IncludedMinutes, null, null, 0, null, "Within minute allowance");
        }

        var overageMinutes = minutes - (plan.IncludedMinutes ?? 0);
        var perMinute = plan.PerMinuteRate ?? 0;
        var charge = overageMinutes * perMinute;

        if (plan.PeakStart.HasValue && plan.PeakEnd.HasValue)
        {
            var timeOfDay = callTime.TimeOfDay;
            if (timeOfDay >= plan.PeakStart.Value && timeOfDay <= plan.PeakEnd.Value)
            {
                var surcharge = charge * (plan.PeakSurcharge ?? 0);
                charge += surcharge;
            }
        }

        return new TelecomRatingResult(
            charge, CurrencyYer, minutes, null, null, plan.IncludedMinutes, null, null, charge, null,
            $"{minutes}min call: {charge} YER");
    }

    public TelecomRatingResult RateSms(int count, RatingPlan plan)
    {
        var perSms = plan.PerSmsRate ?? 0;
        var charge = count * perSms;

        return new TelecomRatingResult(
            charge, CurrencyYer, null, null, count, null, null, null, charge, null,
            $"{count}SMS: {charge} YER");
    }

    public TelecomRatingResult RateWithBundle(
        int usedMinutes, long usedDataBytes, int usedSms,
        BundleAllowance bundle, RatingPlan plan, DateTime? callTime = null)
    {
        var dataResult = RateDataUsage(usedDataBytes, plan);
        var voiceResult = RateVoiceCall(usedMinutes, callTime ?? DateTime.UtcNow, plan);
        var smsResult = RateSms(usedSms, plan);

        var totalCharge = dataResult.Charge + voiceResult.Charge + smsResult.Charge;

        return new TelecomRatingResult(
            totalCharge, CurrencyYer, usedMinutes, usedDataBytes, usedSms,
            plan.IncludedMinutes, plan.IncludedDataBytes, bundle.IncludedSms,
            totalCharge, null,
            $"Bundle: voice {voiceResult.Description}, data {dataResult.Description}, sms {smsResult.Description}");
    }

    public TelecomRatingResult ApplyDiscount(TelecomRatingResult baseCharge, DiscountRule discount)
    {
        var discountAmount = baseCharge.Charge * (discount.Percentage / 100m);

        if (discount.MaxDiscount.HasValue && discountAmount > discount.MaxDiscount.Value)
        {
            discountAmount = discount.MaxDiscount.Value;
        }

        var finalCharge = baseCharge.Charge - discountAmount;

        return baseCharge with
        {
            Charge = finalCharge < 0 ? 0 : finalCharge,
            DiscountApplied = discount.Name,
            OverageCharge = finalCharge
        };
    }
}
