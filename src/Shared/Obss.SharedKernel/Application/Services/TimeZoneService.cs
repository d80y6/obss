namespace Obss.SharedKernel.Application.Services;

public interface ITimeZoneService
{
    DateTime ConvertToLocal(DateTime utcDateTime);
    DateTime ConvertFromLocal(DateTime localDateTime);
    string TimezoneId { get; }
    TimeSpan Offset { get; }
}

public sealed class AdenTimeZoneService : ITimeZoneService
{
    private readonly TimeZoneInfo _timeZone;

    public AdenTimeZoneService()
    {
        _timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Aden");
    }

    public string TimezoneId => "Asia/Aden";
    public TimeSpan Offset => _timeZone.BaseUtcOffset;

    public DateTime ConvertToLocal(DateTime utcDateTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _timeZone);
    }

    public DateTime ConvertFromLocal(DateTime localDateTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, _timeZone);
    }
}
