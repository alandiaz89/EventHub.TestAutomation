namespace EventHub.Core.Models.Events;

public sealed record EventResponse(bool Success, EventItem? Data, string? Message);
