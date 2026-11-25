using EventHub.Core.Models.Events;
using EventHub.Core.TestData;

namespace EventHub.Core.Builders;

public sealed class CreateEventRequestBuilder
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

    public CreateEventRequestBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public CreateEventRequestBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public CreateEventRequestBuilder WithCategory(EventCategory category)
    {
        _category = category.ToString();
        return this;
    }

    public CreateEventRequestBuilder WithVenue(string venue)
    {
        _venue = venue;
        return this;
    }

    public CreateEventRequestBuilder WithCity(string city)
    {
        _city = city;
        return this;
    }

    public CreateEventRequestBuilder WithEventDate(DateTimeOffset eventDate)
    {
        _eventDate = eventDate;
        return this;
    }

    public CreateEventRequestBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public CreateEventRequestBuilder WithTotalSeats(int totalSeats)
    {
        _totalSeats = totalSeats;
        return this;
    }

    public CreateEventRequestBuilder WithImageUrl(string? imageUrl)
    {
        _imageUrl = imageUrl;
        return this;
    }

    public CreateEventRequest Build() =>
        new(_title, _description, _category, _venue, _city, _eventDate, _price, _totalSeats, _imageUrl);
}
