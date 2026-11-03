using Microsoft.Playwright;

namespace EventHub.UiTests.Elements;

public sealed class HeaderElements(IPage page)
{
    private ILocator Root => page.GetByRole(AriaRole.Navigation);
    public ILocator Navbar => Root;
    public ILocator BrandLink => Root.GetByRole(AriaRole.Link, new() { Name = "EventHub", Exact = true });
    public ILocator HomeLink => Root.GetByTestId("nav-home");
    public ILocator EventsLink => Root.GetByTestId("nav-events");
    public ILocator BookingsLink => Root.GetByTestId("nav-bookings");
    public ILocator ApiDocsLink => Root.GetByRole(AriaRole.Link, new() { Name = "API Docs" });
    public ILocator UserEmail => Root.GetByTestId("user-email-display");
    public ILocator LogoutButton => Root.GetByTestId("logout-btn");

    // Only rendered for admin accounts.
    public ILocator AdminButton => Root.GetByRole(AriaRole.Button, new() { Name = "Admin" });
}
