using EventHub.UiTests.Elements;

namespace EventHub.UiTests.Pages.Components;

public sealed class FooterComponent(FooterElements elements)
{
    public async Task ClickBrowseEvents() => await elements.BrowseEventsLink.ClickAsync();

    public async Task ClickMyBookings() => await elements.MyBookingsLink.ClickAsync();

    public async Task ClickManageEvents() => await elements.ManageEventsLink.ClickAsync();
}
