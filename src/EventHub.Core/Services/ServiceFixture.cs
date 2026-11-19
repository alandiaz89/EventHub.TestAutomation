using EventHub.Core.Http;
namespace EventHub.Core.Services;

public sealed class ServiceFixture(ApiClient api)
{
    public AuthService Auth { get; } = new(api);
    public EventsService Events { get; } = new(api);
    public BookingsService Bookings { get; } = new(api);
}
