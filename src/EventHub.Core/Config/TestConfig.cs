using Microsoft.Extensions.Configuration;

namespace EventHub.Core.Config;

public class TestConfig
{
    public string BaseUrl { get; set; } = "";
    public string ApiBaseUrl { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 30;
    public string UserEmail { get; set; } = "";
    public string UserPassword { get; set; } = "";
    public BrowserConfig Browser { get; set; } = new();

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
