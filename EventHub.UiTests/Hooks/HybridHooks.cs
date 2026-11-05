using EventHub.Core.Builders;
using EventHub.Core.Cleanup;
using EventHub.Core.Clients;
using EventHub.Core.Config;
using EventHub.Core.Models.Auth;
using EventHub.Core.Models.Events;

namespace EventHub.UiTests.Hooks;

/// <summary>
/// Base class for hybrid tests: the browser lifecycle of <see cref="PlaywrightHooks"/> plus an API client for setup and
/// cleanup. Pure UI tests keep inheriting <see cref="PlaywrightHooks"/> and pay nothing for the API.
/// NUnit runs base [SetUp] methods first and derived [TearDown] methods first, so the API is ready before the test body
/// and cleanup runs while the browser is still open.
/// </summary>
public abstract class HybridHooks : PlaywrightHooks
{
    private ApiClient? _api;

    protected ClientFixture Clients { get; private set; } = null!;
    protected CleanupRegistry Cleanup { get; } = new();

    [SetUp]
    public void SetUpApi()
    {
        _api = new ApiClient(ApiConfig.From(Config));
        Clients = new ClientFixture(_api);
    }

    [TearDown]
    public async Task TearDownApi()
    {
        try
        {
            var failures = await Cleanup.RunAsync();
            if (failures.Count > 0)
            {
                TestContext.Out.WriteLine($"Cleanup failed for {failures.Count} item(s): {string.Join("; ", failures.Select(f => f.Message))}");
            }
        }
        finally
        {
            _api?.Dispose();
        }
    }

    /// <summary>Registers a new user through the API and fails the test as a precondition if that does not work.</summary>
    protected async Task<AuthInput> RegisterUser()
    {
        var user = new AuthInputBuilder().Build();
        var response = await Clients.Auth.Register(user);
        Assert.That((int)response.StatusCode, Is.EqualTo(201), "Precondition: registration must succeed");
        return user;
    }

    /// <summary>
    /// Cancels, when the test ends, every booking of <paramref name="owner"/>, which gives the seats back to the event.
    /// Only for users registered by the test itself: bookings are private to the account, so nothing else is touched.
    /// </summary>
    protected void CancelBookingsAfterTest(AuthInput owner) =>
        Cleanup.Add(async () =>
        {
            var login = await Clients.Auth.Login(owner);
            var token = login.Data?.Token;
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException($"Could not log in to cancel the bookings: login returned {(int)login.StatusCode}.");
            }

            _api!.SetToken(token);
            var bookings = await Clients.Bookings.GetBookings();
            foreach (var booking in bookings.Data?.Data ?? [])
            {
                await Clients.Bookings.CancelBooking(booking.Id);
            }
        });

    /// <summary>
    /// Deletes, when the test ends, the events owned by <paramref name="owner"/> whose title is exactly
    /// <paramref name="title"/>. For events created through the UI, where the id is not known. Matching the exact title
    /// keeps the shared seeded events safe.
    /// </summary>
    protected void DeleteEventAfterTest(AuthInput owner, string title) =>
        Cleanup.Add(async () =>
        {
            var login = await Clients.Auth.Login(owner);
            var token = login.Data?.Token;
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException($"Could not log in to delete the event '{title}': login returned {(int)login.StatusCode}.");
            }

            _api!.SetToken(token);
            var found = await Clients.Events.GetEvents(new EventsInput { Search = title, Limit = 100 });
            foreach (var item in (found.Data?.Data ?? []).Where(e => e.Title == title))
            {
                await Clients.Events.DeleteEvent(item.Id);
            }
        });
}
