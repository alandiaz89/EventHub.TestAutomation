namespace EventHub.Core.Clients;

public sealed class ClientFixture(ApiClient api)
{
    public AuthClient Auth { get; } = new(api);
}
