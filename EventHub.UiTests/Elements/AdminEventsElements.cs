using Microsoft.Playwright;

namespace EventHub.UiTests.Elements;

public sealed class AdminEventsElements(IPage page)
{
    // New event form
    public ILocator NewEventHeading => page.GetByRole(AriaRole.Heading, new() { Name = "+ New Event" });
    public ILocator EventForm => page.GetByTestId("admin-event-form");
    public ILocator TitleInput => page.GetByTestId("event-title-input");
    public ILocator DescriptionInput => page.GetByPlaceholder("Describe the event…");
    public ILocator CategorySelect => page.GetByLabel("Category");
    public ILocator CityInput => page.GetByLabel("City");
    public ILocator VenueInput => page.GetByLabel("Venue");
    public ILocator DateTimeInput => page.GetByLabel("Event Date & Time");
    public ILocator PriceInput => page.GetByLabel("Price ($)");
    public ILocator TotalSeatsInput => page.GetByLabel("Total Seats");
    public ILocator ImageUrlInput => page.GetByLabel("Image URL");
    public ILocator AddEventButton => page.GetByTestId("add-event-btn");

    // Success pop-up shown after the event is saved
    public ILocator EventCreatedToast => page.GetByText("Event created!");

    // Events table
    public ILocator AllEventsHeading => page.GetByRole(AriaRole.Heading, new() { Name = "All Events" });
    public ILocator EventRows => page.GetByTestId("event-table-row");
    public ILocator EventRow(string title) => EventRows.Filter(new() { HasText = title });
}
