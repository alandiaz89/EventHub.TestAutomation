namespace EventHub.Core.Clients;

public sealed class ClientFixture(ApiClient api)
{
    public AuthClient Auth { get; } = new(api);
    public EventsClient Events { get; } = new(api);
    public BookingsClient Bookings { get; } = new(api);
}
