using FantasyCalendar.Core.Models;
using FantasyCalendar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FantasyCalendar.API.Services;

public class CharacterService : ICharacterService
{
    private readonly AppDbContext _context;

    public CharacterService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Character>> GetCharactersByCalendarAsync(Guid calendarId)
    {
        return await _context.Characters
            .Include(c => c.Unavailabilities)
            .Include(c => c.EventCharacters)
                .ThenInclude(ec => ec.Event)
            .Where(c => c.CalendarId == calendarId)
            .ToListAsync();
    }

    public async Task<Character?> GetCharacterByIdAsync(Guid id)
    {
        return await _context.Characters
            .Include(c => c.Unavailabilities)
            .Include(c => c.EventCharacters)
                .ThenInclude(ec => ec.Event)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Character> CreateCharacterAsync(Guid calendarId, Character character)
    {
        var calendar = await _context.Calendars.FindAsync(calendarId);
        if (calendar == null)
        {
            throw new ArgumentException($"Calendar with ID {calendarId} not found");
        }

        character.Id = Guid.NewGuid();
        character.CalendarId = calendarId;

        _context.Characters.Add(character);
        await _context.SaveChangesAsync();

        return character;
    }

    public async Task<Character?> UpdateCharacterAsync(Guid id, Character updatedCharacter)
    {
        var existingCharacter = await _context.Characters.FindAsync(id);

        if (existingCharacter == null)
        {
            return null;
        }

        existingCharacter.Name = updatedCharacter.Name;
        existingCharacter.Description = updatedCharacter.Description;

        await _context.SaveChangesAsync();

        return existingCharacter;
    }

    public async Task<bool> DeleteCharacterAsync(Guid id)
    {
        var character = await _context.Characters
            .Include(c => c.EventCharacters)
            .Include(c => c.Unavailabilities)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (character == null)
        {
            return false;
        }

        _context.EventCharacters.RemoveRange(character.EventCharacters);
        _context.Unavailabilities.RemoveRange(character.Unavailabilities);
        _context.Characters.Remove(character);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Unavailability> AddUnavailabilityAsync(Guid characterId, Unavailability unavailability)
    {
        var character = await _context.Characters.FindAsync(characterId);
        if (character == null)
        {
            throw new ArgumentException($"Character with ID {characterId} not found");
        }

        // Validate days
        if (unavailability.StartDay < 0)
        {
            throw new ArgumentException("StartDay cannot be negative");
        }

        if (unavailability.EndDay < unavailability.StartDay)
        {
            throw new ArgumentException("EndDay must be greater than or equal to StartDay");
        }

        unavailability.Id = Guid.NewGuid();
        unavailability.CharacterId = characterId;

        _context.Unavailabilities.Add(unavailability);
        await _context.SaveChangesAsync();

        return unavailability;
    }

    public async Task<bool> RemoveUnavailabilityAsync(Guid characterId, Guid unavailabilityId)
    {
        var unavailability = await _context.Unavailabilities
            .FirstOrDefaultAsync(u => u.Id == unavailabilityId && u.CharacterId == characterId);

        if (unavailability == null)
        {
            return false;
        }

        _context.Unavailabilities.Remove(unavailability);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CharacterExistsInCalendarAsync(Guid calendarId, Guid characterId)
    {
        return await _context.Characters
            .AnyAsync(c => c.Id == characterId && c.CalendarId == calendarId);
    }

    public async Task<bool> IsCharacterAvailableAsync(Guid characterId, int startDay, int endDay)
    {
        var character = await _context.Characters
            .Include(c => c.Unavailabilities)
            .FirstOrDefaultAsync(c => c.Id == characterId);

        if (character == null)
        {
            return false;
        }

        return !character.Unavailabilities.Any(u =>
            u.StartDay <= endDay && u.EndDay >= startDay
        );
    }
}