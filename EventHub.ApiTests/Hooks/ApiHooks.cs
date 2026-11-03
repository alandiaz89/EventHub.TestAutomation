using EventHub.Core.Clients;
using EventHub.Core.Config;

namespace EventHub.ApiTests.Hooks;

public abstract class ApiHooks
{
    private ApiClient? _api;

    protected TestConfig Config { get; private set; } = null!;
    protected ClientFixture Clients { get; private set; } = null!;

    [SetUp]
    public void Setup()
    {
        Config = TestConfig.Load();
        _api = new ApiClient(ApiConfig.From(Config));
        Clients = new ClientFixture(_api);
    }

    [TearDown]
    public void TearDown() => _api?.Dispose();
}
