using Obss.SharedKernel.Domain.Common;

namespace Obss.Rating.Domain.ValueObjects;

public sealed class TimeBand : ValueObject
{
    private TimeBand() { }

    public TimeBand(DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, decimal multiplier)
    {
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        Multiplier = multiplier;
    }

    public DayOfWeek DayOfWeek { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public decimal Multiplier { get; private set; }

    public bool IsInBand(DateTime utcTimestamp)
    {
        return utcTimestamp.DayOfWeek == DayOfWeek
            && utcTimestamp.TimeOfDay >= StartTime
            && utcTimestamp.TimeOfDay < EndTime;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return DayOfWeek;
        yield return StartTime;
        yield return EndTime;
        yield return Multiplier;
    }
}
