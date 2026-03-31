using FantasyCalendar.Core.Models;

namespace FantasyCalendar.API.Services;

public interface ICharacterService
{
    Task<IEnumerable<Character>> GetCharactersByCalendarAsync(Guid calendarId);
    Task<Character?> GetCharacterByIdAsync(Guid id);
    Task<Character> CreateCharacterAsync(Guid calendarId, Character character);
    Task<Character?> UpdateCharacterAsync(Guid id, Character updatedCharacter);
    Task<bool> DeleteCharacterAsync(Guid id);
    Task<Unavailability> AddUnavailabilityAsync(Guid characterId, Unavailability unavailability);
    Task<bool> RemoveUnavailabilityAsync(Guid characterId, Guid unavailabilityId);
    Task<bool> CharacterExistsInCalendarAsync(Guid calendarId, Guid characterId);
    Task<bool> IsCharacterAvailableAsync(Guid characterId, int startDay, int endDay);
}