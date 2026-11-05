using EventHub.Core.Models.Events;
using EventHub.Core.TestData;

namespace EventHub.Core.Builders;

public sealed class CreateEventInputBuilder
{
    private string _title = $"Api Event {Guid.NewGuid():N}";
    // Defaults are picked at random from EventData so generated events vary; tests override only what they assert on.
    private string? _description = EventData.Random(EventData.Descriptions);
    private string _category = nameof(EventCategory.Conference);
    private DateTimeOffset _eventDate = DateTimeOffset.UtcNow.AddDays(30);
    private string _venue = EventData.Random(EventData.Venues);
    private string _city = EventData.Random(EventData.Cities);
    private decimal _price = EventData.Random(EventData.Prices);
    private int _totalSeats = EventData.Random(EventData.TotalSeats);

    private string? _imageUrl = EventData.Random(EventData.ImageUrls);

    public CreateEventInputBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public CreateEventInputBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public CreateEventInputBuilder WithCategory(EventCategory category)
    {
        _category = category.ToString();
        return this;
    }

    public CreateEventInputBuilder WithVenue(string venue)
    {
        _venue = venue;
        return this;
    }

    public CreateEventInputBuilder WithCity(string city)
    {
        _city = city;
        return this;
    }

    public CreateEventInputBuilder WithEventDate(DateTimeOffset eventDate)
    {
        _eventDate = eventDate;
        return this;
    }

    public CreateEventInputBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public CreateEventInputBuilder WithTotalSeats(int totalSeats)
    {
        _totalSeats = totalSeats;
        return this;
    }

    public CreateEventInputBuilder WithImageUrl(string? imageUrl)
    {
        _imageUrl = imageUrl;
        return this;
    }

    public CreateEventInput Build() =>
        new(_title, _description, _category, _venue, _city, _eventDate, _price, _totalSeats, _imageUrl);
}
