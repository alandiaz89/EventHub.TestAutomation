using System.Text;
using Allure.Net.Commons;
using EventHub.Core.Cleanup;
using EventHub.Core.Clients;
using EventHub.Core.Config;
using EventHub.Core.Models.Auth;
using EventHub.Core.Models.Events;

namespace EventHub.ApiTests.Hooks;

/// <summary>
/// Base class for API fixtures. NUnit reuses one fixture instance for all of its tests, so the per-test state below
/// (<c>_api</c>, <see cref="Clients"/>, <see cref="Cleanup"/>) is only safe because tests inside a fixture run one at a
/// time (parallelism is at fixture level, see AssemblyInfo). Do not enable ParallelScope.Children without changing this.
/// </summary>
public abstract class ApiHooks
{
    // Read-only after loading, so it is safe to share between fixtures running in parallel.
    private static readonly Lazy<TestConfig> SharedConfig = new(TestConfig.Load);

    private ApiClient? _api;

    protected TestConfig Config => SharedConfig.Value;
    protected ClientFixture Clients { get; private set; } = null!;
    protected CleanupRegistry Cleanup { get; } = new();

    [SetUp]
    public void Setup()
    {
        _api = new ApiClient(ApiConfig.From(Config));
        Clients = new ClientFixture(_api);
    }

    [TearDown]
    public async Task TearDown()
    {
        try
        {
            ReportCallsIfFailed();

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

    /// <summary>Deletes the event through the API when the test ends. Ignores null (request was rejected).</summary>
    protected void TrackEvent(EventItem? created)
    {
        if (created is null)
        {
            return;
        }

        var id = created.Id;
        Cleanup.Add(async () =>
        {
            var response = await Clients.Events.DeleteEvent(id);
            var status = (int)response.StatusCode;

            // 404 means the test already deleted it; anything else that is not 2xx means the event leaked.
            if (status is not (>= 200 and < 300) && status != 404)
            {
                throw new InvalidOperationException($"Event {id} was not deleted: DELETE returned {status}.");
            }
        });
    }

    protected void UseToken(string token) => _api!.SetToken(token);

    protected async Task LoginAsConfiguredUser()
    {
        var (email, password) = Config.RequireUserCredentials();
        await LoginAs(new AuthInput(email, password));
    }

    /// <summary>Logs in and uses the token for the following requests. Fails the test as a precondition if login fails.</summary>
    protected async Task LoginAs(AuthInput input)
    {
        var response = await Clients.Auth.Login(input);
        var token = response.Data?.Token;

        if ((int)response.StatusCode != 200 || string.IsNullOrEmpty(token))
        {
            Assert.Fail($"Precondition failed: login returned {(int)response.StatusCode}.");
        }

        UseToken(token!);
    }

    /// <summary>On failure, prints every request sent (never tokens or auth bodies) and attaches them to the Allure report.</summary>
    private void ReportCallsIfFailed()
    {
        if (_api is null || _api.Calls.Count == 0 || TestContext.CurrentContext.Result.Outcome.Status != NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            return;
        }

        var text = string.Join(Environment.NewLine, _api.Calls.Select(c => c.ToString()));
        TestContext.Out.WriteLine($"API calls:{Environment.NewLine}{text}");
        AllureApi.AddAttachment("API calls", "text/plain", Encoding.UTF8.GetBytes(text), ".txt");
    }
}
