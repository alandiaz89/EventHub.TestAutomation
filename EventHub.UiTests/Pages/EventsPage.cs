using EventHub.UiTests.Elements;

namespace EventHub.UiTests.Pages
{
    public sealed class EventsPage(EventsElements eventsElements)
    {
        public async Task SearchEvent(string eventName)
        {
            await eventsElements.SearchInput.FillAsync(eventName);
            await eventsElements.SearchInput.PressAsync("Enter");
        }

        public async Task ClickEventTitle(string eventName)
        {
            await eventsElements.EventTitleLink(eventName).ClickAsync();
        }

        public async Task ClickEventBookNowLink(string eventName)
        {
            await eventsElements.BookNowLink(eventName).ClickAsync();
        }

        public async Task ClickAddNewEventLink()
        {
            await eventsElements.AddNewEventButton.ClickAsync();
        }
    }
}