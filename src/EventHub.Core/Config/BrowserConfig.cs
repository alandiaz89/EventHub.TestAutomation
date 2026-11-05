namespace EventHub.Core.Config;

public class BrowserConfig
{
    public string Engine { get; init; } = "chromium";
    public bool Headless { get; init; } = true;
    public string[] Args { get; init; } = [];
    public float SlowMo { get; init; }
    public int TimeoutSeconds { get; init; } = 30;
    public bool IgnoreHttpsErrors { get; init; } = true;
}
