namespace Obss.Rating.Domain.ValueObjects;

public sealed record RatingPlan
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public decimal? MonthlyFee { get; init; }
    public int? IncludedMinutes { get; init; }
    public long? IncludedDataBytes { get; init; }
    public decimal? PerMinuteRate { get; init; }
    public decimal? PerMbRate { get; init; }
    public decimal? PerSmsRate { get; init; }
    public decimal? PeakSurcharge { get; init; }
    public TimeSpan? PeakStart { get; init; }
    public TimeSpan? PeakEnd { get; init; }
    public int? IncludedSms { get; init; }
}
