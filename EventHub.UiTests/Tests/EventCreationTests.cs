using Allure.NUnit;
using Allure.NUnit.Attributes;
using EventHub.Core.Builders;
using EventHub.UiTests.Hooks;
using static Microsoft.Playwright.Assertions;

namespace EventHub.UiTests.Tests;

[TestFixture]
[AllureNUnit]
[Category("UI")]
[Category("Events")]
[Category("E2E")]
public class EventCreationTests : HybridHooks
{
    [Test]
    [AllureName("A newly registered user creates an event and finds it by name")]
    public async Task CreateEvent_AsNewlyRegisteredUser_EventIsCreatedAndFoundBySearch()
    {
        var user = await RegisterUser();
        var newEvent = new CreateEventInputBuilder().WithTitle($"UI Event {Guid.NewGuid():N}").Build();
        DeleteEventAfterTest(user, newEvent.Title);

        // Act and assert along the flow
        await Pages.LoginPage.Login(user.Email, user.Password);
        await Expect(Elements.HeaderElements.UserEmail).ToHaveTextAsync(user.Email);

        await Pages.Header.ClickEvents();
        await Expect(Elements.EventsElements.Heading).ToBeVisibleAsync();

        await Pages.EventsPage.ClickAddNewEventLink();
        await Expect(Elements.AdminEventsElements.NewEventHeading).ToBeVisibleAsync();

        await Pages.AdminEventsPage.CreateEvent(newEvent);
        await Expect(Elements.AdminEventsElements.EventCreatedToast).ToBeVisibleAsync();

        await Pages.Header.ClickHome();
        await Expect(Elements.HomeElements.Heading).ToBeVisibleAsync();

        await Pages.HomePage.ClickBrowseEventsLink();
        await Expect(Elements.EventsElements.Heading).ToBeVisibleAsync();

        await Pages.EventsPage.SearchEvent(newEvent.Title);
        await Expect(Elements.EventsElements.EventCard(newEvent.Title)).ToBeVisibleAsync();
    }
}
