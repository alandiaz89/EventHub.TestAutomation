using EventHub.UiTests.Elements;

namespace EventHub.UiTests.Pages
{
    public sealed class RegisterPage(RegisterElements registerElements)
    {
        public async Task Register(string email, string password, string confirmPassword)
        {
            await registerElements.EmailInput.FillAsync(email);
            await registerElements.PasswordInput.FillAsync(password);
            await registerElements.ConfirmPasswordInput.FillAsync(confirmPassword);
            await registerElements.CreateAccountButton.ClickAsync();
        }

        public async Task ClickSignInLink()
        {
            await registerElements.SignInLink.ClickAsync();
        }
    }
}