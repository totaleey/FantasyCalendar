using FantasyCalendar.API.DTOs;

namespace FantasyCalendar.API.Services;

public interface IConflictService
{
    Task<ConflictCheckResponse> CheckEventConflictsAsync(Guid eventId, List<Guid>? characterIds = null);
    Task<bool> HasEventOverlapAsync(Guid calendarId, int startDay, int endDay, Guid? excludeEventId = null);
    Task<List<CharacterConflict>> CheckCharacterAvailabilityForEventAsync(Guid eventId, List<Guid>? characterIds = null);
}