using FantasyCalendar.Core.Models;
using FantasyCalendar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FantasyCalendar.API.Services;

public class EventService : IEventService
{
    private readonly AppDbContext _context;

    public EventService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Event>> GetEventsByCalendarAsync(Guid calendarId)
    {
        return await _context.Events
            .Include(e => e.Recurrence)
            .Include(e => e.EventCharacters)
                .ThenInclude(ec => ec.Character)
            .Where(e => e.CalendarId == calendarId)
            .ToListAsync();
    }

    public async Task<Event?> GetEventByIdAsync(Guid id)
    {
        return await _context.Events
            .Include(e => e.Recurrence)
            .Include(e => e.EventCharacters)
                .ThenInclude(ec => ec.Character)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Event> CreateEventAsync(Guid calendarId, Event newEvent)
    {
        var calendar = await _context.Calendars.FindAsync(calendarId);
        if (calendar == null)
        {
            throw new ArgumentException($"Calendar with ID {calendarId} not found");
        }

        newEvent.Id = Guid.NewGuid();
        newEvent.CalendarId = calendarId;

        if (newEvent.Recurrence != null)
        {
            newEvent.Recurrence.Id = Guid.NewGuid();
            newEvent.Recurrence.EventId = newEvent.Id;
        }

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        return newEvent;
    }

    public async Task<Event?> UpdateEventAsync(Guid id, Event updatedEvent)
    {
        var existingEvent = await _context.Events
            .Include(e => e.Recurrence)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (existingEvent == null)
        {
            return null;
        }

        existingEvent.Title = updatedEvent.Title;
        existingEvent.Description = updatedEvent.Description;
        existingEvent.StartDay = updatedEvent.StartDay;
        existingEvent.EndDay = updatedEvent.EndDay;

        if (updatedEvent.Recurrence != null)
        {
            if (existingEvent.Recurrence != null)
            {
                existingEvent.Recurrence.Type = updatedEvent.Recurrence.Type;
                existingEvent.Recurrence.Interval = updatedEvent.Recurrence.Interval;
                existingEvent.Recurrence.DayOfMonth = updatedEvent.Recurrence.DayOfMonth;
                existingEvent.Recurrence.DayOfWeek = updatedEvent.Recurrence.DayOfWeek;
                existingEvent.Recurrence.UntilDate = updatedEvent.Recurrence.UntilDate;
                existingEvent.Recurrence.MaxOccurrences = updatedEvent.Recurrence.MaxOccurrences;
            }
            else
            {
                existingEvent.Recurrence = updatedEvent.Recurrence;
                existingEvent.Recurrence.Id = Guid.NewGuid();
                existingEvent.Recurrence.EventId = existingEvent.Id;
            }
        }
        else if (existingEvent.Recurrence != null)
        {
            _context.RecurrencePatterns.Remove(existingEvent.Recurrence);
            existingEvent.Recurrence = null;
        }

        await _context.SaveChangesAsync();

        return existingEvent;
    }

    public async Task<bool> DeleteEventAsync(Guid id)
    {
        var eventEntity = await _context.Events.FindAsync(id);

        if (eventEntity == null)
        {
            return false;
        }

        _context.Events.Remove(eventEntity);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<int>> ExpandRecurrenceAsync(Event recurringEvent, int maxDays = 1000)
    {
        if (recurringEvent.Recurrence == null)
        {
            return new List<int> { recurringEvent.StartDay };
        }

        var occurrences = new List<int>();
        var recurrence = recurringEvent.Recurrence;
        var currentDay = recurringEvent.StartDay;
        var endDay = recurrence.UntilDate.HasValue
            ? recurringEvent.StartDay + (recurrence.UntilDate.Value - DateTime.UtcNow).Days // Simplified
            : maxDays;

        endDay = Math.Min(endDay, maxDays);

        var occurrenceCount = 0;

        while (currentDay <= endDay)
        {
            if (occurrenceCount >= recurrence.MaxOccurrences)
            {
                break;
            }

            occurrences.Add(currentDay);
            occurrenceCount++;

            currentDay = GetNextOccurrenceDay(currentDay, recurrence);
        }

        return occurrences;
    }

    private int GetNextOccurrenceDay(int currentDay, RecurrencePattern recurrence)
    {
        return recurrence.Type switch
        {
            RecurrenceType.Daily => currentDay + recurrence.Interval,
            RecurrenceType.Weekly => currentDay + (recurrence.Interval * 7),
            RecurrenceType.Monthly => currentDay + (recurrence.Interval * 30),
            RecurrenceType.Yearly => currentDay + (recurrence.Interval * 360),
            _ => currentDay + recurrence.Interval
        };
    }

    public async Task<bool> EventExistsInCalendarAsync(Guid calendarId, Guid eventId)
    {
        return await _context.Events
            .AnyAsync(e => e.Id == eventId && e.CalendarId == calendarId);
    }
}