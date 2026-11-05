using Microsoft.Extensions.Configuration;

namespace EventHub.Core.Config;

public class TestConfig
{
    public string BaseUrl { get; init; } = "";
    public string ApiBaseUrl { get; init; } = "";
    public int TimeoutSeconds { get; init; } = 30;
    public string UserEmail { get; init; } = "";
    public string UserPassword { get; init; } = "";
    public BrowserConfig Browser { get; init; } = new();

    public (string Email, string Password) RequireUserCredentials() =>
        string.IsNullOrWhiteSpace(UserEmail) || string.IsNullOrWhiteSpace(UserPassword)
            ? throw new InvalidOperationException("Set the UserEmail and UserPassword environment variables.")
            : (UserEmail, UserPassword);

    public static TestConfig Load()
    {
        var env = Environment.GetEnvironmentVariable("TEST_ENV");

        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddJsonFile("appsettings.local.json", optional: true)
            .AddEnvironmentVariables()
            .Build()
            .Get<TestConfig>() ?? new TestConfig();
    }
}
