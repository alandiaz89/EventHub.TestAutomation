namespace EventHub.Core.TestData;

/// <summary>
/// Valid values for the event fields the API accepts. <see cref="Builders.CreateEventInputBuilder"/> picks from these,
/// so each generated event looks different. Title, category and event date are not here: the builder generates them
/// (unique title, enum category, a future date).
/// </summary>
public static class EventData
{
    public static readonly IReadOnlyList<string> Descriptions =
    [
        "A full day of talks and hands-on sessions for software testers.",
        "Live music from local and touring artists on two stages.",
        "Friendly community match open to all skill levels.",
        "Small-group workshop with practical exercises and take-home material.",
        "Food, music and street performances for the whole family.",
        "Networking evening with short lightning talks and refreshments.",
        "Annual meetup covering new tools, techniques and lessons learned.",
        "Open-air show with food trucks and a late-night closing act.",
        "Beginner-friendly bootcamp that covers the basics in one afternoon.",
        "Charity event with proceeds donated to local education programs."
    ];

    public static readonly IReadOnlyList<string> Venues =
    [
        "Grand Convention Center",
        "Riverside Amphitheatre",
        "City Sports Arena",
        "Innovation Hub",
        "Lakeview Pavilion",
        "Central Exhibition Hall",
        "Sunset Park Grounds",
        "Metro Concert Hall",
        "Tech Park Auditorium",
        "Heritage Community Center"
    ];

    public static readonly IReadOnlyList<string> Cities =
    [
        "New York",
        "Los Angeles",
        "Chicago",
        "Houston",
        "Phoenix",
        "Philadelphia",
        "San Antonio",
        "San Diego",
        "Dallas",
        "San Francisco"
    ];

    // Non-negative; includes a free event and prices with cents.
    public static readonly IReadOnlyList<decimal> Prices = [0m, 10m, 25m, 49.99m, 75m, 100m, 150m, 250m, 499.5m, 999m];

    // Positive integers well inside what the API accepts (1 up to 10000 were seen to work).
    public static readonly IReadOnlyList<int> TotalSeats = [5, 10, 20, 25, 50, 75, 100, 200, 500, 1000];

    // Placeholder URLs: the API only checks they are valid http(s) URLs and stores the string.
    public static readonly IReadOnlyList<string> ImageUrls =
    [
        "https://example.com/images/api-automation-event.png",
        "https://example.com/images/conference.jpg",
        "https://example.com/images/concert.jpg",
        "https://example.com/images/sports.jpg",
        "https://example.com/images/workshop.jpg",
        "https://example.com/images/festival.jpg",
        "https://example.com/images/venue-hall.jpg",
        "https://example.com/images/outdoor-stage.jpg",
        "https://example.com/images/community.jpg",
        "https://example.com/images/networking.jpg"
    ];

    public static T Random<T>(IReadOnlyList<T> options) => options[System.Random.Shared.Next(options.Count)];
}
