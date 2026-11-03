using Microsoft.Playwright;

namespace EventHub.UiTests.Elements;

public sealed class HomeElements(IPage page)
{
    public ILocator Heading => page.GetByRole(AriaRole.Heading, new() { Name = "Discover & Book Amazing Events" });
    public ILocator BrowseEventsLink => page.GetByRole(AriaRole.Link, new() { Name = "Browse Events →" });
    public ILocator MyBookingsLink => page.GetByRole(AriaRole.Main).GetByRole(AriaRole.Link, new() { Name = "My Bookings" });

    // Featured events
    public ILocator FeaturedEventsHeading => page.GetByRole(AriaRole.Heading, new() { Name = "Featured Events" });
    public ILocator ViewAllEventsLink => page.GetByRole(AriaRole.Link, new() { Name = "View all →" });
    public ILocator EventCards => page.GetByTestId("event-card");
    public ILocator EventCard(string title) => EventCards.Filter(new() { HasText = title });
    public ILocator EventTitleLink(string title) => EventCard(title).GetByRole(AriaRole.Link, new() { Name = title });
    public ILocator BookNowLink(string title) => EventCard(title).GetByTestId("book-now-btn");

    // Call to action
    public ILocator CallToActionHeading => page.GetByRole(AriaRole.Heading, new() { Name = "Ready to experience something new?" });
    public ILocator ExploreAllEventsButton => page.GetByRole(AriaRole.Button, new() { Name = "Explore All Events" });
    public ILocator MyBookingsButton => page.GetByRole(AriaRole.Button, new() { Name = "My Bookings" });
}
