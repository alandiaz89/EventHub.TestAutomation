using EventHub.UiTests.Elements;
using EventHub.UiTests.Hooks;
using EventHub.UiTests.Pages;
using EventHub.UiTests.Pages.Components;
using static Microsoft.Playwright.Assertions;

namespace EventHub.UiTests.Tests;

[TestFixture]
[Category("UI")]
[Category("Auth")]
public class AuthTests : PlaywrightHooks
{
    private LoginPage loginPage => Pages.LoginPage;
    private HeaderComponent header => Pages.Header;
    private (string Email, string Password) user => Config.RequireUserCredentials();

    [Test, Order(1)]
    public async Task LoginWithValidCredentials()
    {
        await loginPage.Login(user.Email, user.Password);
        await Expect(Elements.HeaderElements.UserEmail).ToHaveTextAsync(user.Email);
    }

    [Test, Order(2)]
    public async Task LoginWithInvalidEmail()
    {
        await loginPage.Login("Test", user.Password);
        await Expect(Elements.LoginElements.InvalidEmailError).ToBeVisibleAsync();
    }

    [Test, Order(3)]
    public async Task LoginWithInvalidPassword()
    {
        await loginPage.Login(user.Email, "Test");
        await Expect(Elements.LoginElements.InvalidPasswordError).ToBeVisibleAsync();
    }

    [Test, Order(4)]
    public async Task Logout()
    {

        await loginPage.Login(user.Email, user.Password);
        await Expect(Elements.HeaderElements.UserEmail).ToHaveTextAsync(user.Email);
        await header.Logout();
        await Expect(Elements.LoginElements.Heading).ToBeVisibleAsync();
    }
}
