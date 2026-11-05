using Allure.NUnit;
using Allure.NUnit.Attributes;
using EventHub.UiTests.Hooks;
using static Microsoft.Playwright.Assertions;

namespace EventHub.UiTests.Tests;

[TestFixture]
[AllureNUnit]
[Category("UI")]
[Category("Bookings")]
[Category("E2E")]
public class BookingTests : HybridHooks
{
    [Test]
    [AllureName("A user books the first event and cancels the booking")]
    public async Task BookFirstEvent_ThenCancelBooking_ShowsNoBookingsYet()
    {
        var user = await RegisterUser();
        CancelBookingsAfterTest(user);

        await Pages.LoginPage.Login(user.Email, user.Password);
        await Expect(Elements.HeaderElements.UserEmail).ToHaveTextAsync(user.Email);

        await Pages.Header.ClickEvents();
        var eventTitle = await Elements.EventsElements.FirstEventTitle.InnerTextAsync();
        // The list is shared and changes, so the first event may have no seats. Fail with a clear reason instead of a timeout.
        var firstCardText = await Elements.EventsElements.FirstEventCard.InnerTextAsync();
        Assert.That(firstCardText, Does.Match(@"[1-9]\d* seats? (available|left)"),
            $"Precondition: the first event '{eventTitle}' must have seats available to book");
        await Pages.EventsPage.ClickFirstEventBookNow();
        await Expect(Elements.EventDetailsElements.TitleHeading(eventTitle)).ToBeVisibleAsync();

        await Pages.EventDetailsPage.BookTickets("Test Customer", user.Email, "+91 98765 43210");
        await Expect(Elements.EventDetailsElements.BookingConfirmedHeading).ToBeVisibleAsync();

        await Pages.Header.ClickBookings();
        await Expect(Elements.BookingsElements.BookingCard(eventTitle)).ToBeVisibleAsync();

        await Pages.BookingsPage.ClickCancelBooking(eventTitle);
        await Pages.BookingsPage.ClickConfirmCancellation();
        await Expect(Elements.BookingsElements.BookingCancelledToast).ToBeVisibleAsync();
        await Expect(Elements.BookingsElements.NoBookingsHeading).ToBeVisibleAsync();
    }
}
