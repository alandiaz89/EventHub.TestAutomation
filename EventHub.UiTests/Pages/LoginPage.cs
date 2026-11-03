using EventHub.UiTests.Elements;

namespace EventHub.UiTests.Pages
{
    public sealed class LoginPage(LoginElements loginElements)
    {
        public async Task Login(string email, string password)
        {
            await loginElements.EmailInput.FillAsync(email);
            await loginElements.PasswordInput.FillAsync(password);
            await loginElements.SignInButton.ClickAsync();
        }

        public async Task ClickRegisterLink()
        {
            await loginElements.RegisterLink.ClickAsync();
        }
    }
}