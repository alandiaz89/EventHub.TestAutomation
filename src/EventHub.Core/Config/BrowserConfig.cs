namespace EventHub.Core.Config;

public class BrowserConfig
{
    public string Engine { get; set; } = "chromium";
    public bool Headless { get; set; } = true;
    public string[] Args { get; set; } = [];
    public float SlowMo { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public bool IgnoreHttpsErrors { get; set; } = true;
}
