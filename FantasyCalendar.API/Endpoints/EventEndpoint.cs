using FantasyCalendar.API.DTOs;
using FantasyCalendar.API.Services;
using FantasyCalendar.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace FantasyCalendar.API.Endpoints;

public static class EventEndpoints
{
    public static void MapEventEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api")
            .WithTags("Events")
            .WithOpenApi();

        // GET /api/calendars/{calendarId}/events
        group.MapGet("/calendars/{calendarId}/events", GetEventsByCalendar)
            .WithName("GetEventsByCalendar")
            .WithDescription("Returns all events for a specific calendar");

        // GET /api/events/{id}
        group.MapGet("/events/{id}", GetEventById)
            .WithName("GetEventById")
            .WithDescription("Returns a specific event with recurrence details");

        // POST /api/calendars/{calendarId}/events
        group.MapPost("/calendars/{calendarId}/events", CreateEvent)
            .WithName("CreateEvent")
            .WithDescription("Creates a new event (one-time or recurring)");

        // PUT /api/events/{id}
        group.MapPut("/events/{id}", UpdateEvent)
            .WithName("UpdateEvent")
            .WithDescription("Updates an existing event");

        // DELETE /api/events/{id}
        group.MapDelete("/events/{id}", DeleteEvent)
            .WithName("DeleteEvent")
            .WithDescription("Deletes an event");

        // POST /api/events/{id}/expand
        group.MapPost("/events/{id}/expand", ExpandRecurrence)
            .WithName("ExpandRecurrence")
            .WithDescription("Expands a recurring event to show all occurrence days");

        // POST /api/events/{id}/check-conflicts
        group.MapPost("/events/{id}/check-conflicts", CheckConflicts)
            .WithName("CheckEventConflicts")
            .WithDescription("Checks if an event conflicts with other events or character availability");

        // Also add this endpoint for checking character availability separately
        group.MapPost("/events/{id}/check-character-availability", CheckCharacterAvailability)
            .WithName("CheckCharacterAvailability")
            .WithDescription("Checks if assigned characters are available for an event");

        // POST /api/events/{id}/characters/{characterId}
        group.MapPost("/events/{id}/characters/{characterId}", AssignCharacter)
            .WithName("AssignCharacter")
            .WithDescription("Assigns a character to an event");

        // DELETE /api/events/{id}/characters/{characterId}
        group.MapDelete("/events/{id}/characters/{characterId}", RemoveCharacter)
            .WithName("RemoveCharacter")
            .WithDescription("Removes a character from an event");

        // GET /api/events/{id}/characters
        group.MapGet("/events/{id}/characters", GetAssignedCharacters)
            .WithName("GetAssignedCharacters")
            .WithDescription("Gets all characters assigned to an event");
    }

    private static async Task<IResult> GetEventsByCalendar(
        Guid calendarId,
        IEventService eventService,
        ICalendarService calendarService)
    {
        // Verify calendar exists
        var calendarExists = await calendarService.CalendarExistsAsync(calendarId);
        if (!calendarExists)
        {
            return Results.NotFound($"Calendar with ID {calendarId} not found");
        }

        var events = await eventService.GetEventsByCalendarAsync(calendarId);

        var response = events.Select(e => new EventSummaryResponse(
            e.Id,
            e.Title,
            e.StartDay,
            e.EndDay,
            e.Recurrence != null
        ));

        return Results.Ok(response);
    }

    private static async Task<IResult> GetEventById(
        Guid id,
        IEventService eventService)
    {
        var eventEntity = await eventService.GetEventByIdAsync(id);

        if (eventEntity == null)
        {
            return Results.NotFound($"Event with ID {id} not found");
        }

        var response = MapToEventResponse(eventEntity);

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateEvent(
        Guid calendarId,
        CreateEventRequest request,
        IEventService eventService,
        ICalendarService calendarService)
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

        var calendarExists = await calendarService.CalendarExistsAsync(calendarId);
        if (!calendarExists)
        {
            return Results.NotFound($"Calendar with ID {calendarId} not found");
        }

        var newEvent = new Event
        {
            Title = request.Title,
            Description = request.Description,
            StartDay = request.StartDay,
            EndDay = request.EndDay,
            Recurrence = request.Recurrence != null ? new RecurrencePattern
            {
                Type = request.Recurrence.Type,
                Interval = request.Recurrence.Interval,
                DayOfMonth = request.Recurrence.DayOfMonth,
                DayOfWeek = request.Recurrence.DayOfWeek,
                UntilDate = request.Recurrence.UntilDate,
                MaxOccurrences = request.Recurrence.MaxOccurrences
            } : null
        };

        try
        {
            var created = await eventService.CreateEventAsync(calendarId, newEvent);
            var response = MapToEventResponse(created);

            return Results.Created($"/api/events/{created.Id}", response);
        }
        catch (ArgumentException ex)
        {
            return Results.NotFound(ex.Message);
        }
    }

    private static async Task<IResult> UpdateEvent(
        Guid id,
        UpdateEventRequest request,
        IEventService eventService)
    {
        var existingEvent = await eventService.GetEventByIdAsync(id);

        if (existingEvent == null)
        {
            return Results.NotFound($"Event with ID {id} not found");
        }

        var updatedEvent = new Event
        {
            Title = request.Title,
            Description = request.Description,
            StartDay = request.StartDay,
            EndDay = request.EndDay,
            Recurrence = request.Recurrence != null ? new RecurrencePattern
            {
                Type = request.Recurrence.Type,
                Interval = request.Recurrence.Interval,
                DayOfMonth = request.Recurrence.DayOfMonth,
                DayOfWeek = request.Recurrence.DayOfWeek,
                UntilDate = request.Recurrence.UntilDate,
                MaxOccurrences = request.Recurrence.MaxOccurrences
            } : null
        };

        var result = await eventService.UpdateEventAsync(id, updatedEvent);

        if (result == null)
        {
            return Results.NotFound($"Event with ID {id} not found");
        }

        var response = MapToEventResponse(result);

        return Results.Ok(response);
    }

    private static async Task<IResult> DeleteEvent(
        Guid id,
        IEventService eventService)
    {
        var result = await eventService.DeleteEventAsync(id);

        if (!result)
        {
            return Results.NotFound($"Event with ID {id} not found");
        }

        return Results.NoContent();
    }

    private static async Task<IResult> ExpandRecurrence(
        Guid id,
        IEventService eventService)
    {
        var eventEntity = await eventService.GetEventByIdAsync(id);

        if (eventEntity == null)
        {
            return Results.NotFound($"Event with ID {id} not found");
        }

        if (eventEntity.Recurrence == null)
        {
            return Results.BadRequest("This event does not have recurrence pattern");
        }

        var occurrences = await eventService.ExpandRecurrenceAsync(eventEntity);

        var response = new ExpandedOccurrencesResponse(
            eventEntity.Id,
            eventEntity.Title,
            occurrences,
            occurrences.Count
        );

        return Results.Ok(response);
    }

    private static EventResponse MapToEventResponse(Event eventEntity)
    {
        return new EventResponse(
            eventEntity.Id,
            eventEntity.Title,
            eventEntity.Description,
            eventEntity.StartDay,
            eventEntity.EndDay,
            eventEntity.Recurrence != null ? new RecurrenceResponse(
                eventEntity.Recurrence.Type,
                eventEntity.Recurrence.Interval,
                eventEntity.Recurrence.DayOfMonth,
                eventEntity.Recurrence.DayOfWeek,
                eventEntity.Recurrence.UntilDate,
                eventEntity.Recurrence.MaxOccurrences
            ) : null,
            eventEntity.EventCharacters?.Select(ec => new CharacterSummaryResponse(
                ec.Character.Id,
                ec.Character.Name
            )).ToList() ?? new List<CharacterSummaryResponse>(),
            eventEntity.CalendarId
        );
    }

    private static async Task<IResult> CheckConflicts(
    Guid id,
    [FromBody] ConflictCheckRequest? request,
    IConflictService conflictService)
    {
        var characterIds = request?.CharacterIds;

        try
        {
            var result = await conflictService.CheckEventConflictsAsync(id, characterIds);
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.NotFound(ex.Message);
        }
    }

    private static async Task<IResult> CheckCharacterAvailability(
        Guid id,
        [FromBody] List<Guid>? characterIds,
        IConflictService conflictService)
    {
        try
        {
            var result = await conflictService.CheckCharacterAvailabilityForEventAsync(id, characterIds);
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Results.NotFound(ex.Message);
        }
    }

    private static async Task<IResult> AssignCharacter(
    Guid id,
    Guid characterId,
    IEventService eventService)
    {
        try
        {
            await eventService.AssignCharacterAsync(id, characterId);
            return Results.NoContent();
        }
        catch (ArgumentException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> RemoveCharacter(
        Guid id,
        Guid characterId,
        IEventService eventService)
    {
        try
        {
            await eventService.RemoveCharacterAsync(id, characterId);
            return Results.NoContent();
        }
        catch (ArgumentException ex)
        {
            return Results.NotFound(ex.Message);
        }
    }

    private static async Task<IResult> GetAssignedCharacters(
        Guid id,
        IEventService eventService)
    {
        try
        {
            var characters = await eventService.GetAssignedCharactersAsync(id);
            var response = characters.Select(c => new CharacterSummaryResponse(c.Id, c.Name));
            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return Results.NotFound(ex.Message);
        }
    }

}