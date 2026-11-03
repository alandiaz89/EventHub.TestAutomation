using EventHub.Core.Models.Auth;

namespace EventHub.Core.Builders;

public sealed class AuthInputBuilder
{
    private string _email = $"api.user.{Guid.NewGuid():N}@example.com";
    private string _password = "Test@1234";

    public AuthInputBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public AuthInputBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    public AuthInput Build() => new(_email, _password);
}
