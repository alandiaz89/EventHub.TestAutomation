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
    // Matches the card whose heading is exactly this title. A text filter would also match any title that contains it.
    public ILocator EventCard(string title) => EventCards.Filter(new() { Has = page.GetByRole(AriaRole.Heading, new() { Name = title, Exact = true }) });
    public ILocator EventTitleLink(string title) => EventCard(title).GetByRole(AriaRole.Link, new() { Name = title, Exact = true });
    public ILocator FirstEventCard => EventCards.First;
    public ILocator FirstEventTitle => FirstEventCard.GetByRole(AriaRole.Heading);
    public ILocator FirstBookNowLink => FirstEventCard.GetByTestId("book-now-btn");
    public ILocator BookNowLink(string title) => EventCard(title).GetByTestId("book-now-btn");
}
