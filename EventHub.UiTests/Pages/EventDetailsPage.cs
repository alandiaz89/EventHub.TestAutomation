using EventHub.UiTests.Elements;

namespace EventHub.UiTests.Pages;

public sealed class EventDetailsPage(EventDetailsElements eventDetailsElements)
{
    public async Task BookTickets(string fullName, string email, string phone)
    {
        await eventDetailsElements.FullNameInput.FillAsync(fullName);
        await eventDetailsElements.EmailInput.FillAsync(email);
        await eventDetailsElements.PhoneInput.FillAsync(phone);
        await eventDetailsElements.ConfirmBookingButton.ClickAsync();
    }
}
