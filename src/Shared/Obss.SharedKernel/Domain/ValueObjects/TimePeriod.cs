namespace Obss.SharedKernel.Domain.ValueObjects;

public sealed record TimePeriod(DateTime? StartDateTime, DateTime? EndDateTime)
{
    public bool IsActive()
    {
        var now = DateTime.UtcNow;
        if (StartDateTime.HasValue && now < StartDateTime.Value)
            return false;
        if (EndDateTime.HasValue && now > EndDateTime.Value)
            return false;
        return true;
    }
}
