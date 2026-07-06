namespace Obss.EventManagement.Domain.ValueObjects;

public sealed record EventFilter
{
    private EventFilter() { }

    public EventFilter(
        string eventType,
        string? filterCriteria = null)
    {
        EventType = eventType;
        FilterCriteria = filterCriteria;
    }

    public string EventType { get; private set; } = string.Empty;
    public string? FilterCriteria { get; private set; }
}
