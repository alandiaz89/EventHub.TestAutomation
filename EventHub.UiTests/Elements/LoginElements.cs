using Microsoft.Playwright;

namespace EventHub.UiTests.Elements;

public sealed class LoginElements(IPage page)
{
    public ILocator Heading => page.GetByRole(AriaRole.Heading, new() { Name = "Sign in to EventHub" });
    public ILocator EmailInput => page.GetByLabel("Email");
    public ILocator PasswordInput => page.GetByLabel("Password");
    public ILocator SignInButton => page.GetByRole(AriaRole.Button, new() { Name = "Sign In" });
    public ILocator RegisterLink => page.GetByRole(AriaRole.Link, new() { Name = "Register" });

    //Invalid credentials
    public ILocator InvalidCredentialsToast => page.GetByText("Invalid email or password");
    public ILocator InvalidEmailError => page.GetByText("Enter a valid email");
    public ILocator InvalidPasswordError => page.GetByText("Password must be at least 6");
}
