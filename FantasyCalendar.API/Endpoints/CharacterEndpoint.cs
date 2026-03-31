using FantasyCalendar.API.DTOs;
using FantasyCalendar.API.Services;
using FantasyCalendar.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCalendar.API.Endpoints;

public static class CharacterEndpoints
{
    public static void MapCharacterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api")
            .WithTags("Characters")
            .WithOpenApi();

        // GET /api/calendars/{calendarId}/characters
        group.MapGet("/calendars/{calendarId}/characters", GetCharactersByCalendar)
            .WithName("GetCharactersByCalendar")
            .WithDescription("Returns all characters for a specific calendar");

        // GET /api/characters/{id}
        group.MapGet("/characters/{id}", GetCharacterById)
            .WithName("GetCharacterById")
            .WithDescription("Returns a specific character with their unavailability and events");

        // POST /api/calendars/{calendarId}/characters
        group.MapPost("/calendars/{calendarId}/characters", CreateCharacter)
            .WithName("CreateCharacter")
            .WithDescription("Creates a new character for a calendar");

        // PUT /api/characters/{id}
        group.MapPut("/characters/{id}", UpdateCharacter)
            .WithName("UpdateCharacter")
            .WithDescription("Updates an existing character");

        // DELETE /api/characters/{id}
        group.MapDelete("/characters/{id}", DeleteCharacter)
            .WithName("DeleteCharacter")
            .WithDescription("Deletes a character and all their unavailability/event associations");

        // POST /api/characters/{id}/unavailability
        group.MapPost("/characters/{id}/unavailability", AddUnavailability)
            .WithName("AddUnavailability")
            .WithDescription("Adds a period of unavailability for a character");

        // DELETE /api/characters/{characterId}/unavailability/{unavailabilityId}
        group.MapDelete("/characters/{characterId}/unavailability/{unavailabilityId}", RemoveUnavailability)
            .WithName("RemoveUnavailability")
            .WithDescription("Removes a period of unavailability from a character");

        // GET /api/characters/{id}/availability
        group.MapGet("/characters/{id}/availability", CheckAvailability)
            .WithName("CheckAvailability")
            .WithDescription("Checks if a character is available during a date range");
    }

    private static async Task<IResult> GetCharactersByCalendar(
        Guid calendarId,
        ICharacterService characterService,
        ICalendarService calendarService)
    {
        // Verify calendar exists
        var calendarExists = await calendarService.CalendarExistsAsync(calendarId);
        if (!calendarExists)
        {
            return Results.NotFound($"Calendar with ID {calendarId} not found");
        }

        var characters = await characterService.GetCharactersByCalendarAsync(calendarId);

        var response = characters.Select(c => new CharacterSummaryResponse(
            c.Id,
            c.Name
        ));

        return Results.Ok(response);
    }

    private static async Task<IResult> GetCharacterById(
        Guid id,
        ICharacterService characterService)
    {
        var character = await characterService.GetCharacterByIdAsync(id);

        if (character == null)
        {
            return Results.NotFound($"Character with ID {id} not found");
        }

        var response = MapToCharacterResponse(character);

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateCharacter(
        Guid calendarId,
        CreateCharacterRequest request,
        ICharacterService characterService,
        ICalendarService calendarService)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest("Character name is required");
        }

        var calendarExists = await calendarService.CalendarExistsAsync(calendarId);
        if (!calendarExists)
        {
            return Results.NotFound($"Calendar with ID {calendarId} not found");
        }

        var newCharacter = new Character
        {
            Name = request.Name,
            Description = request.Description ?? string.Empty
        };

        try
        {
            var created = await characterService.CreateCharacterAsync(calendarId, newCharacter);
            var response = MapToCharacterResponse(created);

            return Results.Created($"/api/characters/{created.Id}", response);
        }
        catch (ArgumentException ex)
        {
            return Results.NotFound(ex.Message);
        }
    }

    private static async Task<IResult> UpdateCharacter(
        Guid id,
        UpdateCharacterRequest request,
        ICharacterService characterService)
    {
        var existingCharacter = await characterService.GetCharacterByIdAsync(id);

        if (existingCharacter == null)
        {
            return Results.NotFound($"Character with ID {id} not found");
        }

        var updatedCharacter = new Character
        {
            Name = request.Name,
            Description = request.Description ?? string.Empty
        };

        var result = await characterService.UpdateCharacterAsync(id, updatedCharacter);

        if (result == null)
        {
            return Results.NotFound($"Character with ID {id} not found");
        }

        var response = MapToCharacterResponse(result);

        return Results.Ok(response);
    }

    private static async Task<IResult> DeleteCharacter(
        Guid id,
        ICharacterService characterService)
    {
        var result = await characterService.DeleteCharacterAsync(id);

        if (!result)
        {
            return Results.NotFound($"Character with ID {id} not found");
        }

        return Results.NoContent();
    }

    private static async Task<IResult> AddUnavailability(
        Guid id,
        AddUnavailabilityRequest request,
        ICharacterService characterService)
    {
        // Validate
        if (request.StartDay < 0)
        {
            return Results.BadRequest("StartDay cannot be negative");
        }

        if (request.EndDay < request.StartDay)
        {
            return Results.BadRequest("EndDay must be greater than or equal to StartDay");
        }

        var unavailability = new Unavailability
        {
            StartDay = request.StartDay,
            EndDay = request.EndDay,
            Reason = request.Reason ?? string.Empty
        };

        try
        {
            var created = await characterService.AddUnavailabilityAsync(id, unavailability);

            var response = new UnavailabilityResponse(
                created.Id,
                created.StartDay,
                created.EndDay,
                created.Reason
            );

            return Results.Created($"/api/characters/{id}/unavailability/{created.Id}", response);
        }
        catch (ArgumentException ex)
        {
            return Results.NotFound(ex.Message);
        }
    }

    private static async Task<IResult> RemoveUnavailability(
        Guid characterId,
        Guid unavailabilityId,
        ICharacterService characterService)
    {
        var result = await characterService.RemoveUnavailabilityAsync(characterId, unavailabilityId);

        if (!result)
        {
            return Results.NotFound($"Unavailability with ID {unavailabilityId} not found for character {characterId}");
        }

        return Results.NoContent();
    }

    private static async Task<IResult> CheckAvailability(
        Guid id,
        [FromQuery] int startDay,
        [FromQuery] int endDay,
        ICharacterService characterService)
    {
        if (startDay < 0 || endDay < startDay)
        {
            return Results.BadRequest("Invalid date range. StartDay must be >= 0 and EndDay >= StartDay");
        }

        var character = await characterService.GetCharacterByIdAsync(id);

        if (character == null)
        {
            return Results.NotFound($"Character with ID {id} not found");
        }

        var isAvailable = await characterService.IsCharacterAvailableAsync(id, startDay, endDay);

        var conflictingUnavailabilities = character.Unavailabilities
            .Where(u => u.StartDay <= endDay && u.EndDay >= startDay)
            .Select(u => new UnavailabilityResponse(u.Id, u.StartDay, u.EndDay, u.Reason))
            .ToList();

        var response = new CharacterAvailabilityResponse(
            character.Id,
            character.Name,
            isAvailable,
            conflictingUnavailabilities
        );

        return Results.Ok(response);
    }

    private static CharacterResponse MapToCharacterResponse(Character character)
    {
        return new CharacterResponse(
            character.Id,
            character.Name,
            character.Description,
            character.Unavailabilities?.Select(u => new UnavailabilityResponse(
                u.Id,
                u.StartDay,
                u.EndDay,
                u.Reason
            )).ToList() ?? new List<UnavailabilityResponse>(),
            character.EventCharacters?.Select(ec => new EventSummaryResponse(
                ec.Event.Id,
                ec.Event.Title,
                ec.Event.StartDay,
                ec.Event.EndDay,
                ec.Event.Recurrence != null
            )).ToList() ?? new List<EventSummaryResponse>()
        );
    }
}