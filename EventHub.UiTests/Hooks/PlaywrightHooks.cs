using Allure.Net.Commons;
using EventHub.Core.Config;
using EventHub.UiTests.Driver;
using EventHub.UiTests.Elements;
using EventHub.UiTests.Pages;
using Microsoft.Playwright;
using NUnit.Framework.Interfaces;

namespace EventHub.UiTests.Hooks;

public abstract class PlaywrightHooks
{
    // Read-only after loading, so it is safe to share between fixtures running in parallel.
    private static readonly Lazy<TestConfig> SharedConfig = new(TestConfig.Load);

    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private bool _tracing;
    private readonly List<string> _browserErrors = [];

    protected IPage Page { get; private set; } = null!;
    protected ElementFixture Elements { get; private set; } = null!;
    protected PageFixture Pages { get; private set; } = null!;

    protected TestConfig Config => SharedConfig.Value;

    [SetUp]
    public async Task Setup()
    {
        _playwright = await Playwright.CreateAsync();
        var factory = new BrowserFactory(Config.Browser);
        _browser = await factory.LaunchAsync(_playwright);
        _context = await factory.CreateContextAsync(_browser);
        await _context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });
        _tracing = true;
        Page = await _context.NewPageAsync();
        Page.Console += (_, message) =>
        {
            if (message.Type == "error") _browserErrors.Add($"[console] {message.Text}");
        };
        Page.PageError += (_, error) => _browserErrors.Add($"[pageerror] {error}");
        Elements = new ElementFixture(Page);
        Pages = new PageFixture(Elements);

        await Page.GotoAsync(Config.BaseUrl);
    }

    [TearDown]
    public async Task TearDown()
    {
        try
        {
            await CollectDiagnostics();
        }
        finally
        {
            // Each step runs even if the one before it throws, so a failed close cannot leak a browser process.
            try
            {
                if (_context != null) await _context.CloseAsync();
            }
            finally
            {
                try
                {
                    if (_browser != null) await _browser.CloseAsync();
                }
                finally
                {
                    _playwright?.Dispose();
                }
            }
        }
    }

    private async Task CollectDiagnostics()
    {
        var failed = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed;

        if (failed && Page != null)
        {
            await RunSafelyAsync("failure screenshot", CaptureScreenshotAsync);
            RunSafely("page URL", AttachPageUrl);
            RunSafely("browser errors", AttachBrowserErrors);
        }

        await RunSafelyAsync("trace", () => StopTracingAsync(keep: failed));
    }

    private async Task CaptureScreenshotAsync() =>
        AllureApi.AddAttachment("Failure screenshot", "image/png", await Page.ScreenshotAsync());

    private void AttachPageUrl() =>
        AllureApi.AddAttachment("Page URL", "text/plain", System.Text.Encoding.UTF8.GetBytes(Page.Url));

    private void AttachBrowserErrors()
    {
        if (_browserErrors.Count == 0) return;

        AllureApi.AddAttachment("Browser errors", "text/plain",
            System.Text.Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, _browserErrors)));
    }

    private async Task StopTracingAsync(bool keep)
    {
        if (!_tracing || _context == null) return;
        _tracing = false;

        if (!keep)
        {
            await _context.Tracing.StopAsync();
            return;
        }

        var tracePath = Path.Combine(AppContext.BaseDirectory, "traces",
            $"{Sanitize(TestContext.CurrentContext.Test.Name)}_{Guid.NewGuid().ToString("N")[..8]}.zip");
        Directory.CreateDirectory(Path.GetDirectoryName(tracePath)!);
        await _context.Tracing.StopAsync(new() { Path = tracePath });
        AllureApi.AddAttachment(tracePath, "Playwright trace");
    }

    private static async Task RunSafelyAsync(string what, Func<Task> operation)
    {
        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Could not collect {what}: {ex.Message}");
        }
    }

    private static void RunSafely(string what, Action operation)
    {
        try
        {
            operation();
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Could not collect {what}: {ex.Message}");
        }
    }

    private static string Sanitize(string name) =>
        string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
}
