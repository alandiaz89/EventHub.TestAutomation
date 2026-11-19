using EventHub.Core.Builders;
using EventHub.Core.Cleanup;
using EventHub.Core.Http;
using EventHub.Core.Services;
using EventHub.Core.Config;
using EventHub.Core.Models.Auth;
using EventHub.Core.Models.Events;

namespace EventHub.UiTests.Hooks;

public abstract class HybridHooks : PlaywrightHooks
{
    private ApiClient? _api;

    protected ServiceFixture Services { get; private set; } = null!;
    protected CleanupRegistry Cleanup { get; } = new();

    [SetUp]
    public void SetUpApi()
    {
        _api = new ApiClient(ApiConfig.From(Config));
        Services = new ServiceFixture(_api);
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

    protected async Task<AuthInput> RegisterUser()
    {
        var user = new AuthInputBuilder().Build();
        var response = await Services.Auth.Register(user);
        Assert.That((int)response.StatusCode, Is.EqualTo(201), "Precondition: registration must succeed");
        return user;
    }

    protected void CancelBookingsAfterTest(AuthInput owner) =>
        Cleanup.Add(async () =>
        {
            var login = await Services.Auth.Login(owner);
            var token = login.Data?.Token;
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException($"Could not log in to cancel the bookings: login returned {(int)login.StatusCode}.");
            }

            _api!.SetToken(token);
            var bookings = await Services.Bookings.GetBookings();
            foreach (var booking in bookings.Data?.Data ?? [])
            {
                await Services.Bookings.CancelBooking(booking.Id);
            }
        });
    
    protected void DeleteEventAfterTest(AuthInput owner, string title) =>
        Cleanup.Add(async () =>
        {
            var login = await Services.Auth.Login(owner);
            var token = login.Data?.Token;
            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException($"Could not log in to delete the event '{title}': login returned {(int)login.StatusCode}.");
            }

            _api!.SetToken(token);
            var found = await Services.Events.GetEvents(new EventsInput { Search = title, Limit = 100 });
            foreach (var item in (found.Data?.Data ?? []).Where(e => e.Title == title))
            {
                await Services.Events.DeleteEvent(item.Id);
            }
        });
}
