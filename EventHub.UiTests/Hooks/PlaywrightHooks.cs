using EventHub.Core.Config;
using EventHub.UiTests.Driver;
using EventHub.UiTests.Elements;
using EventHub.UiTests.Pages;
using Microsoft.Playwright;

namespace EventHub.UiTests.Hooks;

public abstract class PlaywrightHooks
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;

    protected IPage Page { get; private set; } = null!;
    protected ElementFixture Elements { get; private set; } = null!;
    protected PageFixture Pages { get; private set; } = null!;

    protected TestConfig Config { get; private set; } = null!;

    [SetUp]
    public async Task Setup()
    {
        Config = TestConfig.Load();
        _playwright = await Playwright.CreateAsync();
        var factory = new BrowserFactory(Config.Browser);
        _browser = await factory.LaunchAsync(_playwright);
        _context = await factory.CreateContextAsync(_browser);
        Page = await _context.NewPageAsync();
        Elements = new ElementFixture(Page);
        Pages = new PageFixture(Elements);

        await Page.GotoAsync(Config.BaseUrl);
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_context != null) await _context.CloseAsync();
        if (_browser != null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }
}
