using EventHub.UiTests.Elements;

namespace EventHub.UiTests.Pages;

public sealed class BookingsPage(BookingsElements bookingsElements)
{
    public async Task ClickCancelBooking(string eventTitle) =>
        await bookingsElements.CancelBookingButton(eventTitle).ClickAsync();

    public async Task ClickConfirmCancellation() =>
        await bookingsElements.ConfirmCancelButton.ClickAsync();
}
