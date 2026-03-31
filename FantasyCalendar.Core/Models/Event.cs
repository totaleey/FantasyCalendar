namespace FantasyCalendar.Core.Models;

public class Event
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StartDay { get; set; }
    public int EndDay { get; set; }

    public RecurrencePattern? Recurrence { get; set; }

    public Guid CalendarId { get; set; }
    public Calendar Calendar { get; set; } = null!;
    public List<EventCharacter> EventCharacters { get; set; } = new();
}

public class RecurrencePattern
{
    public Guid Id { get; set; }
    public RecurrenceType Type { get; set; }
    public int Interval { get; set; } = 1;
    public int? DayOfMonth { get; set; }
    public int? DayOfWeek { get; set; }
    public DateTime? UntilDate { get; set; }
    public int? MaxOccurrences { get; set; }

    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
}

public enum RecurrenceType
{
    Daily,
    Weekly,
    Monthly,
    Yearly
}