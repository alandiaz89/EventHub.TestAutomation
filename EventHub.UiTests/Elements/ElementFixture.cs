using Microsoft.Playwright;

namespace EventHub.UiTests.Elements;

public sealed class ElementFixture(IPage page)
{
    public LoginElements LoginElements { get; } = new(page);
    public RegisterElements RegisterElements { get; } = new(page);
    public HomeElements HomeElements { get; } = new(page);
    public HeaderElements HeaderElements { get; } = new(page);
    public FooterElements FooterElements { get; } = new(page);
}
