using Microsoft.Playwright;

namespace EventHub.UiTests.Elements;

public sealed class EventDetailsElements(IPage page)
{
    public ILocator TitleHeading(string title) => page.GetByRole(AriaRole.Heading, new() { Name = title, Level = 1 });

    // Book tickets form
    public ILocator FullNameInput => page.GetByLabel("Full Name");
    public ILocator EmailInput => page.GetByTestId("customer-email");
    public ILocator PhoneInput => page.GetByLabel("Phone Number");
    public ILocator ConfirmBookingButton => page.GetByRole(AriaRole.Button, new() { Name = "Confirm Booking" });

    // Result
    public ILocator BookingConfirmedHeading => page.GetByRole(AriaRole.Heading, new() { Name = "Booking Confirmed!" });
}
