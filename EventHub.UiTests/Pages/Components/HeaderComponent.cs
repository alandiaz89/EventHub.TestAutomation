using EventHub.UiTests.Elements;

namespace EventHub.UiTests.Pages.Components;

public sealed class HeaderComponent(HeaderElements elements)
{
    public async Task ClickHome() => await elements.HomeLink.ClickAsync();

    public async Task ClickEvents() => await elements.EventsLink.ClickAsync();

    public async Task ClickBookings() => await elements.BookingsLink.ClickAsync();

    public async Task ClickAdminMenu() => await elements.AdminButton.ClickAsync();

    public async Task Logout() => await elements.LogoutButton.ClickAsync();
}
