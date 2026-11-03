using EventHub.Core.Config;
using Microsoft.Playwright;

namespace EventHub.UiTests.Driver;
public class BrowserFactory(BrowserConfig config)
{
    public Task<IBrowser> LaunchAsync(IPlaywright playwright)
    {
        var options = new BrowserTypeLaunchOptions
        {
            Headless = config.Headless,
            SlowMo = config.SlowMo,
            Timeout = config.TimeoutSeconds * 1000,
            Args = config.Args
        };

        switch (config.Engine.ToLowerInvariant())
        {
            case "firefox":
                return playwright.Firefox.LaunchAsync(options);
            case "webkit":
                return playwright.Webkit.LaunchAsync(options);
            case "chrome":
                options.Channel = "chrome";
                return playwright.Chromium.LaunchAsync(options);
            case "msedge":
                options.Channel = "msedge";
                return playwright.Chromium.LaunchAsync(options);
            case "chromium":
                return playwright.Chromium.LaunchAsync(options);
            default:
                throw new NotSupportedException($"Browser engine '{config.Engine}' is not supported.");
        }
    }

    public async Task<IBrowserContext> CreateContextAsync(IBrowser browser)
    {
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = config.IgnoreHttpsErrors,
            ViewportSize = ViewportSize.NoViewport
        });
        context.SetDefaultTimeout(config.TimeoutSeconds * 1000);
        return context;
    }
}
