namespace FantasyCalendar.Core.Models;

public class Calendar
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int DaysPerYear { get; set; } = 365;
    public int MonthsPerYear { get; set; } = 12;
    public int DaysPerWeek { get; set; } = 7;

    public List<Month> Months { get; set; } = new();
    public List<Weekday> Weekdays { get; set; } = new();
    public List<Event> Events { get; set; } = new();
    public List<Character> Characters { get; set; } = new();
}

public class Month
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public int DaysInMonth { get; set; }
    public Guid CalendarId { get; set; }
    public Calendar Calendar { get; set; } = null!;
}

public class Weekday
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public Guid CalendarId { get; set; }
    public Calendar Calendar { get; set; } = null!;
}