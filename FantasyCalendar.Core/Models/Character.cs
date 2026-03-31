namespace FantasyCalendar.Core.Models;

public class Character
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid CalendarId { get; set; }
    public Calendar Calendar { get; set; } = null!;

    public List<Unavailability> Unavailabilities { get; set; } = new();
    public List<EventCharacter> EventCharacters { get; set; } = new();
}

public class Unavailability
{
    public Guid Id { get; set; }
    public int StartDay { get; set; }
    public int EndDay { get; set; }
    public string Reason { get; set; } = string.Empty;

    public Guid CharacterId { get; set; }
    public Character Character { get; set; } = null!;
}

public class EventCharacter
{
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;

    public Guid CharacterId { get; set; }
    public Character Character { get; set; } = null!;
}