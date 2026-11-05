using Microsoft.Playwright;

namespace EventHub.UiTests.Elements;

public sealed class BookingsElements(IPage page)
{
    public ILocator Heading => page.GetByRole(AriaRole.Heading, new() { Name = "My Bookings" });
    public ILocator ClearAllBookingsButton => page.GetByRole(AriaRole.Button, new() { Name = "Clear all bookings" });

    // Booking cards
    public ILocator BookingCards => page.GetByTestId("booking-card");
    public ILocator BookingCard(string eventTitle) => BookingCards.Filter(new() { HasText = eventTitle });
    public ILocator BookingId(string eventTitle) => BookingCard(eventTitle).GetByTestId("booking-id");
    public ILocator ViewDetailsLink(string eventTitle) => BookingCard(eventTitle).GetByRole(AriaRole.Link, new() { Name = "View Details" });
    public ILocator CancelBookingButton(string eventTitle) => BookingCard(eventTitle).GetByTestId("cancel-booking-btn");

    // Cancel confirmation dialog and result
    public ILocator CancelDialog => page.GetByRole(AriaRole.Dialog);
    public ILocator ConfirmCancelButton => CancelDialog.GetByTestId("confirm-dialog-yes");
    public ILocator BookingCancelledToast => page.GetByText("Booking cancelled successfully");
    public ILocator NoBookingsHeading => page.GetByRole(AriaRole.Heading, new() { Name = "No bookings yet" });
}
