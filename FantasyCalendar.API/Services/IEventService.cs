using FantasyCalendar.Core.Models;

namespace FantasyCalendar.API.Services;

public interface IEventService
{
    Task<IEnumerable<Event>> GetEventsByCalendarAsync(Guid calendarId);
    Task<Event?> GetEventByIdAsync(Guid id);
    Task<Event> CreateEventAsync(Guid calendarId, Event newEvent);
    Task<Event?> UpdateEventAsync(Guid id, Event updatedEvent);
    Task<bool> DeleteEventAsync(Guid id);
    Task<List<int>> ExpandRecurrenceAsync(Event recurringEvent, int maxDays = 1000);
    Task<bool> EventExistsInCalendarAsync(Guid calendarId, Guid eventId);
    Task<List<int>> GetEventDaysAsync(Event eventEntity, int maxDays = 1000);
    Task AssignCharacterAsync(Guid eventId, Guid characterId);
    Task RemoveCharacterAsync(Guid eventId, Guid characterId);
    Task<List<Character>> GetAssignedCharactersAsync(Guid eventId);
    Task<bool> IsCharacterAssignedAsync(Guid eventId, Guid characterId);
}