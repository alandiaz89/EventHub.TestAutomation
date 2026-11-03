using EventHub.UiTests.Elements;

namespace EventHub.UiTests.Pages
{
    public sealed class HomePage(HomeElements homeElements)
    {
        public async Task ClickBrowseEventsLink()
        {
            await homeElements.BrowseEventsLink.ClickAsync();
        }
        
        public async Task ClickMyBookingsLink()
        {
            await homeElements.MyBookingsLink.ClickAsync();
        }

        public async Task ClickViewAllLink()
        {
            await homeElements.ViewAllEventsLink.ClickAsync();
        }
    }
}