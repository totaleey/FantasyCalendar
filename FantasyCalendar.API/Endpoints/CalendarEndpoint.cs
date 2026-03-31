using FantasyCalendar.API.DTOs;
using FantasyCalendar.API.Services;
using FantasyCalendar.Core.Models;

namespace FantasyCalendar.API.Endpoints;

public static class CalendarEndpoints
{
    public static void MapCalendarEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/calendars")
            .WithTags("Calendars")
            .WithOpenApi();

        // GET /api/calendars - List all calendars
        group.MapGet("/", GetAllCalendars)
            .WithName("GetAllCalendars")
            .WithDescription("Returns a list of all calendars (summary view without months/weekdays)");

        // GET /api/calendars/{id} - Get single calendar
        group.MapGet("/{id}", GetCalendarById)
            .WithName("GetCalendarById")
            .WithDescription("Returns a specific calendar with its months and weekdays");

        // POST /api/calendars - Create new calendar
        group.MapPost("/", CreateCalendar)
            .WithName("CreateCalendar")
            .WithDescription("Creates a new custom calendar");

        // PUT /api/calendars/{id} - Update calendar
        group.MapPut("/{id}", UpdateCalendar)
            .WithName("UpdateCalendar")
            .WithDescription("Updates an existing calendar");

        // DELETE /api/calendars/{id} - Delete calendar
        group.MapDelete("/{id}", DeleteCalendar)
            .WithName("DeleteCalendar")
            .WithDescription("Deletes a calendar and all its associated data (events, characters, etc.)");
    }

    private static async Task<IResult> GetAllCalendars(
        ICalendarService calendarService)
    {
        var calendars = await calendarService.GetAllCalendarsAsync(includeDetails: false);

        var response = calendars.Select(c => new CalendarSummaryResponse(
            c.Id,
            c.Name,
            c.Description
        ));

        return Results.Ok(response);
    }

    private static async Task<IResult> GetCalendarById(
        Guid id,
        ICalendarService calendarService)
    {
        var calendar = await calendarService.GetCalendarByIdAsync(id, includeDetails: true);

        if (calendar is null)
        {
            return Results.NotFound($"Calendar with ID {id} not found");
        }

        var response = new CalendarResponse(
            calendar.Id,
            calendar.Name,
            calendar.Description,
            calendar.DaysPerYear,
            calendar.MonthsPerYear,
            calendar.DaysPerWeek,
            calendar.Months?.Select(m => new MonthResponse(m.Id, m.Name, m.Order, m.DaysInMonth)).ToList() ?? new List<MonthResponse>(),
            calendar.Weekdays?.Select(w => new WeekdayResponse(w.Id, w.Name, w.Order)).ToList() ?? new List<WeekdayResponse>()
        );

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateCalendar(
        CreateCalendarRequest request,
        ICalendarService calendarService)
    {
        // Basic validation
        if (request.MonthsPerYear <= 0 || request.DaysPerYear <= 0 || request.DaysPerWeek <= 0)
        {
            return Results.BadRequest("DaysPerYear, MonthsPerYear, and DaysPerWeek must be positive numbers");
        }

        var calendar = new Calendar
        {
            Name = request.Name,
            Description = request.Description,
            DaysPerYear = request.DaysPerYear,
            MonthsPerYear = request.MonthsPerYear,
            DaysPerWeek = request.DaysPerWeek
        };

        var created = await calendarService.CreateCalendarAsync(calendar);

        var response = new CalendarSummaryResponse(
            created.Id,
            created.Name,
            created.Description
        );

        return Results.Created($"/api/calendars/{created.Id}", response);
    }

    private static async Task<IResult> UpdateCalendar(
        Guid id,
        UpdateCalendarRequest request,
        ICalendarService calendarService)
    {
        var existing = await calendarService.GetCalendarByIdAsync(id, includeDetails: false);

        if (existing is null)
        {
            return Results.NotFound($"Calendar with ID {id} not found");
        }

        var updatedCalendar = new Calendar
        {
            Name = request.Name,
            Description = request.Description,
            DaysPerYear = request.DaysPerYear,
            MonthsPerYear = request.MonthsPerYear,
            DaysPerWeek = request.DaysPerWeek
        };

        var result = await calendarService.UpdateCalendarAsync(id, updatedCalendar);

        if (result is null)
        {
            return Results.NotFound($"Calendar with ID {id} not found");
        }

        var response = new CalendarSummaryResponse(
            result.Id,
            result.Name,
            result.Description
        );

        return Results.Ok(response);
    }

    private static async Task<IResult> DeleteCalendar(
        Guid id,
        ICalendarService calendarService)
    {
        var exists = await calendarService.CalendarExistsAsync(id);

        if (!exists)
        {
            return Results.NotFound($"Calendar with ID {id} not found");
        }

        await calendarService.DeleteCalendarAsync(id);

        return Results.NoContent();
    }
}