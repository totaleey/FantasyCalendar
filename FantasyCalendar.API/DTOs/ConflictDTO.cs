namespace FantasyCalendar.API.DTOs;

public record ConflictCheckRequest(
    Guid EventId,
    List<Guid>? CharacterIds  // Optional: specific characters to check
);

public record ConflictCheckResponse(
    bool HasConflicts,
    List<EventConflict> EventConflicts,
    List<CharacterConflict> CharacterConflicts
);

public record EventConflict(
    Guid ConflictingEventId,
    string ConflictingEventTitle,
    int OverlapStartDay,
    int OverlapEndDay
);

public record CharacterConflict(
    Guid CharacterId,
    string CharacterName,
    bool IsAvailable,
    List<UnavailabilityResponse> ConflictingUnavailabilities
);