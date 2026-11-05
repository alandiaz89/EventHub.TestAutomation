using System.Globalization;
using EventHub.Core.Models.Events;
using EventHub.UiTests.Elements;

namespace EventHub.UiTests.Pages;

public sealed class AdminEventsPage(AdminEventsElements adminEventsElements)
{    public async Task CreateEvent(CreateEventInput input)
    {
        await adminEventsElements.TitleInput.FillAsync(input.Title);
        await adminEventsElements.CategorySelect.SelectOptionAsync(input.Category);
        await adminEventsElements.CityInput.FillAsync(input.City);
        await adminEventsElements.VenueInput.FillAsync(input.Venue);
        await adminEventsElements.DateTimeInput.FillAsync(input.EventDate.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm", CultureInfo.InvariantCulture));
        await adminEventsElements.PriceInput.FillAsync(input.Price.ToString(CultureInfo.InvariantCulture));
        await adminEventsElements.TotalSeatsInput.FillAsync(input.TotalSeats.ToString(CultureInfo.InvariantCulture));
        await adminEventsElements.AddEventButton.ClickAsync();
    }
}
