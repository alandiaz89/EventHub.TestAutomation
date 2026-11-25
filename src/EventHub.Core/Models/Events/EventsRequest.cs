namespace EventHub.Core.Models.Events;

public sealed class EventsRequest
{
    public EventCategory? Category { get; init; }
    /// <summary>Category sent as raw text (overrides <see cref="Category"/>); for casing, empty and invalid-value tests.</summary>
    public string? CategoryText { get; init; }
    public string? City { get; init; }
    public string? Search { get; init; }
    public int? Page { get; init; }
    public int? Limit { get; init; }

    public Dictionary<string, object?> ToQuery() => new()
    {
        ["category"] = CategoryText ?? Category?.ToString(),
        ["city"] = City,
        ["search"] = Search,
        ["page"] = Page,
        ["limit"] = Limit
    };
}
