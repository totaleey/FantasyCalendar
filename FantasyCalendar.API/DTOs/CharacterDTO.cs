using FantasyCalendar.Core.Models;

namespace FantasyCalendar.API.DTOs;

// Request DTOs
public record CreateCharacterRequest(
    string Name,
    string Description
);

public record UpdateCharacterRequest(
    string Name,
    string Description
);

public record AddUnavailabilityRequest(
    int StartDay,
    int EndDay,
    string Reason
);

// Response DTOs
public record CharacterResponse(
    Guid Id,
    string Name,
    string Description,
    List<UnavailabilityResponse> Unavailabilities,
    List<EventSummaryResponse> Events
);

public record UnavailabilityResponse(
    Guid Id,
    int StartDay,
    int EndDay,
    string Reason
);

// For conflict detection (we'll use this later)
public record CharacterAvailabilityResponse(
    Guid CharacterId,
    string CharacterName,
    bool IsAvailable,
    List<UnavailabilityResponse> ConflictingUnavailabilities
);

public record AssignCharacterRequest(
    Guid CharacterId
);

public record EventCharacterResponse(
    Guid EventId,
    string EventTitle,
    List<CharacterSummaryResponse> Characters
);