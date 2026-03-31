namespace FantasyCalendar.API.DTOs;

// Request DTOs
public record CreateCalendarRequest(
    string Name,
    string Description,
    int DaysPerYear,
    int MonthsPerYear,
    int DaysPerWeek
);

public record UpdateCalendarRequest(
    string Name,
    string Description,
    int DaysPerYear,
    int MonthsPerYear,
    int DaysPerWeek
);

// Response DTOs
public record CalendarResponse(
    Guid Id,
    string Name,
    string Description,
    int DaysPerYear,
    int MonthsPerYear,
    int DaysPerWeek,
    List<MonthResponse> Months,
    List<WeekdayResponse> Weekdays
);

public record MonthResponse(
    Guid Id,
    string Name,
    int Order,
    int DaysInMonth
);

public record WeekdayResponse(
    Guid Id,
    string Name,
    int Order
);

public record CalendarSummaryResponse(
    Guid Id,
    string Name,
    string Description
);