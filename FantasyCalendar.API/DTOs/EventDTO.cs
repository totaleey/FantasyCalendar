using FantasyCalendar.Core.Models;

namespace FantasyCalendar.API.DTOs;

// Request DTOs
public record CreateEventRequest(
    string Title,
    string Description,
    int StartDay,
    int EndDay,
    RecurrenceRequest? Recurrence
);

public record UpdateEventRequest(
    string Title,
    string Description,
    int StartDay,
    int EndDay,
    RecurrenceRequest? Recurrence
);

public record RecurrenceRequest(
    RecurrenceType Type,
    int Interval,
    int? DayOfMonth,
    int? DayOfWeek,
    DateTime? UntilDate,
    int? MaxOccurrences
);

// Response DTOs
public record EventResponse(
    Guid Id,
    string Title,
    string Description,
    int StartDay,
    int EndDay,
    RecurrenceResponse? Recurrence,
    List<CharacterSummaryResponse> Characters,
    Guid CalendarId
);

public record RecurrenceResponse(
    RecurrenceType Type,
    int Interval,
    int? DayOfMonth,
    int? DayOfWeek,
    DateTime? UntilDate,
    int? MaxOccurrences
);

public record EventSummaryResponse(
    Guid Id,
    string Title,
    int StartDay,
    int EndDay,
    bool IsRecurring
);

// For recurrence expansion
public record ExpandedOccurrencesResponse(
    Guid EventId,
    string Title,
    List<int> OccurrenceDays,
    int TotalOccurrences
);

public record CharacterSummaryResponse(
    Guid Id,
    string Name
);