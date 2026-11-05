namespace EventHub.Core.Models.Events;

public sealed record EventsResponse(bool Success, IReadOnlyList<EventItem>? Data, Pagination? Pagination);

public sealed record EventItem(
    int Id,
    string Title,
    string? Description,
    string Category,
    string? Venue,
    string? City,
    DateTimeOffset EventDate,
    string? Price,
    int TotalSeats,
    int AvailableSeats,
    string? ImageUrl = null);

public sealed record Pagination(int Total, int Page, int Limit, int TotalPages);
