using EventHub.Core.Models.Events;

namespace EventHub.Core.Models.Bookings;

public sealed record BookingsResponse(bool Success, IReadOnlyList<BookingItem>? Data, Pagination? Pagination);

public sealed record BookingItem(int Id, int EventId, string Status, string? BookingRef);
