using FantasyCalendar.API.DTOs;
using FantasyCalendar.Core.Models;
using FantasyCalendar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FantasyCalendar.API.Services;

public class ConflictService : IConflictService
{
    private readonly AppDbContext _context;
    private readonly IEventService _eventService;
    private readonly ICharacterService _characterService;

    public ConflictService(
        AppDbContext context,
        IEventService eventService,
        ICharacterService characterService)
    {
        _context = context;
        _eventService = eventService;
        _characterService = characterService;
    }

    public async Task<ConflictCheckResponse> CheckEventConflictsAsync(
        Guid eventId,
        List<Guid>? characterIds = null)
    {
        var eventEntity = await _eventService.GetEventByIdAsync(eventId);
        if (eventEntity == null)
        {
            throw new ArgumentException($"Event with ID {eventId} not found");
        }

        var occurrenceDays = await _eventService.GetEventDaysAsync(eventEntity, 500);

        var eventConflicts = new List<EventConflict>();
        var characterConflicts = new List<CharacterConflict>();

        // Check event vs event conflicts
        var overlappingEvents = await GetOverlappingEventsAsync(
            eventEntity.CalendarId,
            occurrenceDays,
            eventId
        );

        foreach (var overlappingEvent in overlappingEvents)
        {
            var overlapRange = GetOverlapRange(occurrenceDays, overlappingEvent);
            eventConflicts.Add(new EventConflict(
                overlappingEvent.Id,
                overlappingEvent.Title,
                overlapRange.StartDay,
                overlapRange.EndDay
            ));
        }

        var charactersToCheck = characterIds ?? eventEntity.EventCharacters?.Select(ec => ec.CharacterId).ToList();

        // If still null, no characters to check
        if (charactersToCheck == null || !charactersToCheck.Any())
        {
            charactersToCheck = new List<Guid>();
        }

        if (charactersToCheck != null && charactersToCheck.Any())
        {
            foreach (var characterId in charactersToCheck)
            {
                var character = await _characterService.GetCharacterByIdAsync(characterId);
                if (character == null) continue;

                var conflicts = new List<UnavailabilityResponse>();

                foreach (var day in occurrenceDays)
                {
                    var isAvailable = await _characterService.IsCharacterAvailableAsync(
                        characterId, day, day);

                    if (!isAvailable)
                    {
                        // Get the specific unavailability for this day
                        var unavailability = character.Unavailabilities
                            .FirstOrDefault(u => u.StartDay <= day && u.EndDay >= day);

                        if (unavailability != null && !conflicts.Any(c => c.Id == unavailability.Id))
                        {
                            conflicts.Add(new UnavailabilityResponse(
                                unavailability.Id,
                                unavailability.StartDay,
                                unavailability.EndDay,
                                unavailability.Reason
                            ));
                        }
                    }
                }

                characterConflicts.Add(new CharacterConflict(
                    character.Id,
                    character.Name,
                    conflicts.Count == 0,
                    conflicts
                ));
            }
        }

        return new ConflictCheckResponse(
            eventConflicts.Any() || characterConflicts.Any(c => !c.IsAvailable),
            eventConflicts,
            characterConflicts
        );
    }

    public async Task<bool> HasEventOverlapAsync(
        Guid calendarId,
        int startDay,
        int endDay,
        Guid? excludeEventId = null)
    {
        var events = await _context.Events
            .Where(e => e.CalendarId == calendarId)
            .ToListAsync();

        if (excludeEventId.HasValue)
        {
            events = events.Where(e => e.Id != excludeEventId.Value).ToList();
        }

        foreach (var eventEntity in events)
        {
            var occurrenceDays = eventEntity.Recurrence != null
                ? await _eventService.ExpandRecurrenceAsync(eventEntity, 500)
                : new List<int> { eventEntity.StartDay };

            if (occurrenceDays.Any(day => day >= startDay && day <= endDay))
            {
                return true;
            }
        }

        return false;
    }

    public async Task<List<CharacterConflict>> CheckCharacterAvailabilityForEventAsync(
        Guid eventId,
        List<Guid>? characterIds = null)
    {
        var eventEntity = await _eventService.GetEventByIdAsync(eventId);
        if (eventEntity == null)
        {
            throw new ArgumentException($"Event with ID {eventId} not found");
        }

        var occurrenceDays = eventEntity.Recurrence != null
            ? await _eventService.ExpandRecurrenceAsync(eventEntity, 500)
            : new List<int> { eventEntity.StartDay };

        var charactersToCheck = characterIds ?? eventEntity.EventCharacters?.Select(ec => ec.CharacterId).ToList();
        var results = new List<CharacterConflict>();

        if (charactersToCheck == null || !charactersToCheck.Any())
        {
            return results;
        }

        foreach (var characterId in charactersToCheck)
        {
            var character = await _characterService.GetCharacterByIdAsync(characterId);
            if (character == null) continue;

            var conflicts = new List<UnavailabilityResponse>();

            foreach (var day in occurrenceDays)
            {
                var isAvailable = await _characterService.IsCharacterAvailableAsync(
                    characterId, day, day);

                if (!isAvailable)
                {
                    var unavailability = character.Unavailabilities
                        .FirstOrDefault(u => u.StartDay <= day && u.EndDay >= day);

                    if (unavailability != null && !conflicts.Any(c => c.Id == unavailability.Id))
                    {
                        conflicts.Add(new UnavailabilityResponse(
                            unavailability.Id,
                            unavailability.StartDay,
                            unavailability.EndDay,
                            unavailability.Reason
                        ));
                    }
                }
            }

            results.Add(new CharacterConflict(
                character.Id,
                character.Name,
                conflicts.Count == 0,
                conflicts
            ));
        }

        return results;
    }

    private async Task<List<Event>> GetOverlappingEventsAsync(
        Guid calendarId,
        List<int> occurrenceDays,
        Guid excludeEventId)
    {
        var allEvents = await _context.Events
            .Where(e => e.CalendarId == calendarId && e.Id != excludeEventId)
            .ToListAsync();

        var overlapping = new List<Event>();

        foreach (var eventEntity in allEvents)
        {
            var eventOccurrences = await _eventService.GetEventDaysAsync(eventEntity, 500);

            if (occurrenceDays.Intersect(eventOccurrences).Any())
            {
                overlapping.Add(eventEntity);
            }
        }

        return overlapping;
    }

    private (int StartDay, int EndDay) GetOverlapRange(List<int> days1, Event event2)
    {
        var event2Days = event2.Recurrence != null
            ? _eventService.ExpandRecurrenceAsync(event2, 500).Result
            : new List<int> { event2.StartDay };

        var overlappingDays = days1.Intersect(event2Days).ToList();

        return overlappingDays.Any()
            ? (overlappingDays.Min(), overlappingDays.Max())
            : (0, 0);
    }
}