namespace EventHub.Core.Models.Events;

public sealed record CreateEventInput(
    string Title,
    string? Description,
    string Category,
    string Venue,
    string City,
    DateTimeOffset EventDate,
    decimal Price,
    int TotalSeats,
    string? ImageUrl = null)
{
    /// <summary>Camel-case JSON payload as a mutable dictionary, so tests can remove a field or send a wrong type.</summary>
    public Dictionary<string, object?> ToPayload() => new()
    {
        ["title"] = Title,
        ["description"] = Description,
        ["category"] = Category,
        ["venue"] = Venue,
        ["city"] = City,
        ["eventDate"] = EventDate.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"),
        ["price"] = Price,
        ["totalSeats"] = TotalSeats,
        ["imageUrl"] = ImageUrl
    };
}
