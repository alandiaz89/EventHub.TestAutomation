using Microsoft.Playwright;

namespace EventHub.UiTests.Elements;

public sealed class RegisterElements(IPage page)
{
    public ILocator Heading => page.GetByRole(AriaRole.Heading, new() { Name = "Create your account" });
    public ILocator EmailInput => page.GetByTestId("register-email");
    public ILocator PasswordInput => page.GetByTestId("register-password");
    public ILocator ConfirmPasswordInput => page.GetByPlaceholder("Repeat your password");
    public ILocator CreateAccountButton => page.GetByRole(AriaRole.Button, new() { Name = "Create Account" });
    public ILocator SignInLink => page.GetByRole(AriaRole.Link, new() { Name = "Sign in" });

    // Password requirements checklist
    public ILocator MinLengthRequirement => page.GetByText("At least 8 characters");
    public ILocator UppercaseRequirement => page.GetByText("One uppercase letter (A–Z)");
    public ILocator NumberRequirement => page.GetByText("One number (0–9)");
    public ILocator SpecialCharacterRequirement => page.GetByText("One special character");

    // Validation messages
    public ILocator InvalidEmailError => page.GetByText("Enter a valid email");
    public ILocator PasswordRequirementsError => page.GetByText("Password does not meet the requirements below");
    public ILocator PasswordMismatchError => page.GetByText("Passwords do not match");
}
