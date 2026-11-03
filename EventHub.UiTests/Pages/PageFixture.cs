using EventHub.UiTests.Elements;
using EventHub.UiTests.Pages.Components;

namespace EventHub.UiTests.Pages;

public sealed class PageFixture(ElementFixture elements)
{
    public LoginPage LoginPage { get; } = new(elements.LoginElements);
    public RegisterPage RegisterPage { get; } = new(elements.RegisterElements);
     public HeaderComponent Header { get; } = new(elements.HeaderElements);
    public HomePage HomePage { get; } = new(elements.HomeElements);
    public EventsPage EventsPage { get; } = new(elements.EventsElements);
    public FooterComponent Footer { get; } = new(elements.FooterElements);
}
