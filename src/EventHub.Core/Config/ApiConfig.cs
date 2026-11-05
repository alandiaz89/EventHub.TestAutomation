namespace EventHub.Core.Config;

public sealed class ApiConfig
{
    public string BaseUrl { get; init; } = "";
    public int TimeoutSeconds { get; init; } = 30;

    public static ApiConfig From(TestConfig config) =>
        Uri.TryCreate(config.ApiBaseUrl, UriKind.Absolute, out _)
            ? new ApiConfig { BaseUrl = config.ApiBaseUrl, TimeoutSeconds = config.TimeoutSeconds }
            : throw new InvalidOperationException(
                $"ApiBaseUrl must be an absolute URL (current value: '{config.ApiBaseUrl}'). Set it in appsettings.json.");
}
