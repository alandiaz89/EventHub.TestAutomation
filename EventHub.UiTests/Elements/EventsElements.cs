using Microsoft.Playwright;

namespace EventHub.UiTests.Elements;

public sealed class EventsElements(IPage page)
{
    public ILocator Heading => page.GetByRole(AriaRole.Heading, new() { Name = "Upcoming Events" });
    public ILocator SearchInput => page.GetByPlaceholder("Search events, venues…");
    public ILocator CategoryFilter => page.GetByRole(AriaRole.Combobox).Filter(new() { Has = page.GetByRole(AriaRole.Option, new() { Name = "All Categories" }) });
    public ILocator CityFilter => page.GetByRole(AriaRole.Combobox).Filter(new() { Has = page.GetByRole(AriaRole.Option, new() { Name = "All Cities" }) });
    public ILocator AddNewEventButton => page.GetByRole(AriaRole.Button, new() { Name = "Add New Event" });

    // Event cards
    public ILocator EventCards => page.GetByTestId("event-card");
    public ILocator EventCard(string title) => EventCards.Filter(new() { HasText = title });
    public ILocator EventTitleLink(string title) => EventCard(title).GetByRole(AriaRole.Link, new() { Name = title });
    public ILocator BookNowLink(string title) => EventCard(title).GetByTestId("book-now-btn");
}
